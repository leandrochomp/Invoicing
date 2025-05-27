using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Invoices.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Invoices.Repositories;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAll(DbTransaction? transaction = null);
    Task<Invoice?> GetInvoiceById(Guid id, DbTransaction? transaction = null);
    Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId, DbTransaction? transaction = null);
    Task<Invoice> CreateInvoice(Invoice invoice, DbTransaction? transaction = null);
    Task<bool> UpdateInvoice(Invoice invoice, DbTransaction? transaction = null);
    Task<bool> DeleteInvoice(Guid id, DbTransaction? transaction = null);
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _dbContext;

    public InvoiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Invoice>> GetAll(DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Invoices.Where(i => !i.IsDeleted).ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceById(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Invoices.Where(i => i.Id == id && !i.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Invoices.Where(i => i.ClientId == clientId && !i.IsDeleted).ToListAsync();
    }

    public async Task<Invoice> CreateInvoice(Invoice invoice, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }
        
        invoice.CreatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.Invoices.AddAsync(invoice);
        await _dbContext.SaveChangesAsync();

        return invoice;
    }

    public async Task<bool> UpdateInvoice(Invoice invoice, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        var existingInvoice = await _dbContext.Invoices
            .Where(i => i.Id == invoice.Id && !i.IsDeleted)
            .FirstOrDefaultAsync();

        if (existingInvoice == null)
        {
            return false;
        }

        // Update properties
        existingInvoice.InvoiceNumber = invoice.InvoiceNumber;
        existingInvoice.ClientId = invoice.ClientId;
        existingInvoice.IssueDate = invoice.IssueDate;
        existingInvoice.DueDate = invoice.DueDate;
        existingInvoice.Status = invoice.Status;
        existingInvoice.TotalAmount = invoice.TotalAmount;
        existingInvoice.Currency = invoice.Currency;
        existingInvoice.Notes = invoice.Notes;
        existingInvoice.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteInvoice(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        var invoice = await _dbContext.Invoices
            .Where(i => i.Id == id && !i.IsDeleted)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return false;
        }

        // Soft delete
        invoice.IsDeleted = true;
        invoice.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.SaveChangesAsync();
        return true;
    }
}