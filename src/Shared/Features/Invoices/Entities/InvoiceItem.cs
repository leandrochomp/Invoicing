using Shared.Features.Common.Entities;

namespace Shared.Features.Invoices.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal Total { get; set; }
    public int? SortOrder { get; set; }

    public Invoice Invoice { get; set; } = default!;
}