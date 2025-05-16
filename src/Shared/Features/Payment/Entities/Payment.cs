using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Shared.Features.Payment.Entities;

public sealed class Payment
{
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }
    
    [ForeignKey("payment_invoice_id_fk")]
    public required Guid InvoiceId { get; set; }
    
    public Invoice.Entities.Invoice Invoice { get; set; }
    
    [Key]
    [Column("amount_paid")]
    public required decimal AmountPaid { get; set; }
    
    [Key]
    [Column("payment_date")]
    public required Instant PaymentDate { get; set; }
    
    [Key]
    [Column("payment_method")]
    public required string PaymentMethod { get; set; }
    
    [Key]
    [Column("reference_number")]
    public string? ReferenceNumber { get; set; }
    
    [Key]
    [Column("notes")]
    public string? Notes { get; set; }
}