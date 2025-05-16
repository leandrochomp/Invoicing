using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Features.Invoice.Entities;

public sealed class InvoiceItem
{
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }
    
    [ForeignKey("invoice_item_invoice_id_fk")]
    public required Guid InvoiceId { get; set; }
    
    [Key]
    [Column("description")]
    public required string Description { get; set; }
    
    [Key]
    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Key]
    [Column("unit_price")]
    public decimal UnitPrice { get; set; }
    
    [Key]
    [Column("tax_rate")]
    public decimal? TaxRate { get; set; }
    
    [Key]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }
    
    [Key]
    [Column("sort_order")]
    public int? SortOrder { get; set; }
}