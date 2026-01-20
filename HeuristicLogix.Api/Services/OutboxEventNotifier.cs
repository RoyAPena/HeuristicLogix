using System.Threading.Channels;

namespace HeuristicLogix.Api.Services;

/// <summary>
/// Service for instant notification of new outbox events using System.Threading.Channels.
/// Eliminates polling latency by signaling the background publisher immediately when events are added.
/// </summary>
public interface IOutboxEventNotifier
{
    /// <summary>
    /// Notifies the background publisher that a new event has been added to the outbox.
    /// </summary>
    ValueTask NotifyEventAddedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for the next event notification.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if notification received, false if channel is closed.</returns>
    ValueTask<bool> WaitForEventAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of outbox event notifier using System.Threading.Channels.
/// Provides instant notification with zero polling delay.
/// </summary>
public class OutboxEventNotifier : IOutboxEventNotifier
{
    private readonly Channel<bool> _notificationChannel;

    public OutboxEventNotifier()
    {
        // Create unbounded channel for event notifications
        // Multiple writers (API calls), single reader (background service)
        _notificationChannel = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
    }

    public async ValueTask NotifyEventAddedAsync(CancellationToken cancellationToken = default)
    {
        // Try to write notification, but don't block if channel is full
        // (shouldn't happen with unbounded channel, but safe guard)
        await _notificationChannel.Writer.WriteAsync(true, cancellationToken);
    }

    public async ValueTask<bool> WaitForEventAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Wait for next notification or cancellation
            bool notification = await _notificationChannel.Reader.ReadAsync(cancellationToken);
            return notification;
        }
        catch (ChannelClosedException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
