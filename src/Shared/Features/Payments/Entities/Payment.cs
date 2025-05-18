using NodaTime;
using Shared.Features.Common.Entities;
using Shared.Features.Invoices.Entities;

namespace Shared.Features.Payments.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal AmountPaid { get; set; }
    public Instant PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    public Invoice Invoice { get; set; } = default!;
}

public enum PaymentMethod
{
    BankTransfer,
    CreditCard,
    Cash,
    Check,
    PayPal,
    Other
}