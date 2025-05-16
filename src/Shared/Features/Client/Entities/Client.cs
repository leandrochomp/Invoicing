using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Features.Common.Entities;

namespace Shared.Features.Client.Entities;

public class Client : BaseEntity
{
    [Key]
    [Column("id")]
    public required Guid Id { get; set; }
    
    [Key]
    [Column("name")]
    public required string Name { get; set; }
    
    [Key]
    [Column("email")]
    public string Email { get; set; }
    
    [Key]
    [Column("company_name")]
    public string? CompanyName { get; set; }
    
    [Key]
    [Column("address")]
    public string? Address { get; set; }
    
    [Key]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }
    
    public IList<Invoice.Entities.Invoice> Invoices { get; set; } = new List<Invoice.Entities.Invoice>();
}