using Microsoft.EntityFrameworkCore;
using Shared.Features.Clients.Entities;
using Shared.Features.Invoices.Entities;
using Shared.Features.Payments.Entities;

namespace Shared.Infrastructure.Data;

public interface IAppDbContext
{
    DbSet<Client> Clients { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(npgsqlOptionsAction: o => o.UseNodaTime());
        }
    }
}