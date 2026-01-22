using System.Collections.Generic;
using HeuristicLogix.Shared.DTOs;

namespace HeuristicLogix.Client.ViewModels;

public sealed class InvoiceValidationViewModel
{
    public required List<InvoiceStagingDto> Invoices { get; init; }
    public required string GoogleMapsFrontendKey { get; init; }
}
