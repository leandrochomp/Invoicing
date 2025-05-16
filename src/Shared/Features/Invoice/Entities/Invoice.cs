using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Shared.Features.Common.Entities;

namespace Shared.Features.Invoice.Entities;

public sealed class Invoice : BaseEntity
{
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }
    
    [ForeignKey("invoice_clientId_fk")]
    public required Guid ClientId { get; set; }
    
    public Client.Entities.Client Client { get; set; }
    
    [Key]
    [Column("invoice_number")]
    public required string InvoiceNumber { get; set; }
    
    [Key]
    [Column("issue_date")]
    public LocalDateTime IssueDate { get; set; }
    
    [Key]
    [Column("due_date")]
    public LocalDateTime DueDate { get; set; }
    
    [Key]
    [Column("status")]
    public string Status { get; set; }
    
    [Key]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }
    
    [Key]
    [Column("currency")]
    public string Currency { get; set; }
    
    [Key]
    [Column("notes")]
    public string? Notes { get; set; }
    
    public IList<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public IList<Payment.Entities.Payment> Payments { get; set; } = new List<Payment.Entities.Payment>();
}