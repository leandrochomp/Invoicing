using NodaTime;
using Shared.Features.Clients.Entities;
using Shared.Features.Common.Entities;
using Shared.Features.Payments.Entities;

namespace Shared.Features.Invoices.Entities;

public class Invoice : BaseEntity
{
    public Guid ClientId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public LocalDateTime IssueDate { get; set; }
    public LocalDateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = default!;
    public string? Notes { get; set; }

    public Client Client { get; set; } = default!;
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    public void UpdateStatusBasedOnPayments(decimal totalPaid)
    {
        Status = DetermineStatus(totalPaid);
    }

    private InvoiceStatus DetermineStatus(decimal totalPaid)
    {
        if (totalPaid >= TotalAmount)
        {
            return InvoiceStatus.Paid;
        }
        
        return totalPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Sent;
    }
    
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Overdue,
    PartiallyPaid,
    Paid,
    Cancelled,
    Disputed
}