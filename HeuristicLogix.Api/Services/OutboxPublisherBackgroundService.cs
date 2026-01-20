using Confluent.Kafka;
using HeuristicLogix.Shared.Models;

namespace HeuristicLogix.Api.Services;

/// <summary>
/// Background service that publishes outbox events to Kafka with instant notification.
/// Uses Channel-based signaling for zero-latency event processing instead of polling.
/// Implements reliable at-least-once delivery semantics.
/// </summary>
public class OutboxPublisherBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOutboxEventNotifier _notifier;
    private readonly TimeSpan _fallbackPollingInterval;
    private readonly int _batchSize;
    private readonly int _maxRetryAttempts;

    public OutboxPublisherBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisherBackgroundService> logger,
        IConfiguration configuration,
        IOutboxEventNotifier notifier)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _notifier = notifier;
        _fallbackPollingInterval = TimeSpan.FromSeconds(
            _configuration.GetValue<int>("OutboxPublisher:FallbackPollingIntervalSeconds", 30));
        _batchSize = _configuration.GetValue<int>("OutboxPublisher:BatchSize", 100);
        _maxRetryAttempts = _configuration.GetValue<int>("OutboxPublisher:MaxRetryAttempts", 3);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox Publisher Background Service started with instant notification (fallback polling: {Interval}s).",
            _fallbackPollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // INSTANT NOTIFICATION: Wait for event signal or fallback timeout
                // This eliminates polling latency - we process events immediately when added
                using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(_fallbackPollingInterval);

                try
                {
                    bool notificationReceived = await _notifier.WaitForEventAsync(timeoutCts.Token);
                    
                    if (notificationReceived)
                    {
                        _logger.LogDebug("Received instant notification - processing outbox events.");
                    }
                    else
                    {
                        _logger.LogDebug("Fallback polling triggered - checking for pending events.");
                    }
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
                {
                    // Fallback timeout reached - process any pending events
                    _logger.LogDebug("Fallback timeout reached - checking for pending events.");
                }


                // Process pending events (whether notified or fallback polling)
                await ProcessOutboxEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox events.");
                // Brief delay before retrying on error
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("Outbox Publisher Background Service stopped.");
    }


    private async Task ProcessOutboxEventsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ITransactionalOutboxService outboxService = scope.ServiceProvider
            .GetRequiredService<ITransactionalOutboxService>();

        List<OutboxEvent> pendingEvents = await outboxService.GetPendingEventsAsync(
            _batchSize, cancellationToken);

        if (pendingEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending outbox events.", pendingEvents.Count);

        string kafkaBootstrapServers = _configuration.GetValue<string>(
            "Kafka:BootstrapServers", "localhost:9092") ?? "localhost:9092";

        ProducerConfig config = new ProducerConfig
        {
            BootstrapServers = kafkaBootstrapServers,
            EnableIdempotence = true,
            MaxInFlight = 5,
            Acks = Acks.All,
            MessageSendMaxRetries = 3
        };

        using IProducer<string, string> producer = new ProducerBuilder<string, string>(config).Build();

        foreach (OutboxEvent outboxEvent in pendingEvents)
        {
            try
            {
                await outboxService.IncrementAttemptAsync(outboxEvent.Id, cancellationToken);

                Message<string, string> message = new Message<string, string>
                {
                    Key = outboxEvent.AggregateId,
                    Value = outboxEvent.PayloadJson,
                    Headers = new Headers
                    {
                        { "event-type", System.Text.Encoding.UTF8.GetBytes(outboxEvent.EventType) },
                        { "event-id", System.Text.Encoding.UTF8.GetBytes(outboxEvent.Id.ToString()) }
                    }
                };

                if (!string.IsNullOrWhiteSpace(outboxEvent.CorrelationId))
                {
                    message.Headers.Add("correlation-id",
                        System.Text.Encoding.UTF8.GetBytes(outboxEvent.CorrelationId));
                }

                DeliveryResult<string, string> result = await producer.ProduceAsync(
                    outboxEvent.Topic, message, cancellationToken);

                if (result.Status == PersistenceStatus.Persisted)
                {
                    await outboxService.MarkAsPublishedAsync(outboxEvent.Id, cancellationToken);
                    _logger.LogInformation(
                        "Published event {EventId} to topic {Topic} (partition {Partition}, offset {Offset}).",
                        outboxEvent.Id, outboxEvent.Topic, result.Partition.Value, result.Offset.Value);
                }
                else
                {
                    string errorMessage = $"Kafka persistence failed with status: {result.Status}";
                    _logger.LogWarning("Event {EventId} publishing failed: {Error}",
                        outboxEvent.Id, errorMessage);

                    if (outboxEvent.AttemptCount >= _maxRetryAttempts)
                    {
                        await outboxService.MarkAsFailedAsync(outboxEvent.Id, errorMessage, cancellationToken);
                    }
                }
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Kafka produce error for event {EventId}: {Error}",
                    outboxEvent.Id, ex.Error.Reason);

                if (outboxEvent.AttemptCount >= _maxRetryAttempts)
                {
                    await outboxService.MarkAsFailedAsync(
                        outboxEvent.Id, ex.Error.Reason, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error publishing event {EventId}", outboxEvent.Id);

                if (outboxEvent.AttemptCount >= _maxRetryAttempts)
                {
                    await outboxService.MarkAsFailedAsync(
                        outboxEvent.Id, ex.Message, cancellationToken);
                }
            }
        }
    }
}
