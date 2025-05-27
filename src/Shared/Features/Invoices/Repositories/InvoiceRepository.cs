using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Invoices.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Invoices.Repositories;

/// <summary>
/// Provides data access operations for invoices including retrieval, creation, update, and deletion.
/// Handles database interactions and transaction management for invoice-related operations.
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Retrieves all active (non-deleted) invoices.
    /// </summary>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>A collection of all active invoices.</returns>
    Task<IEnumerable<Invoice>> GetAll(DbTransaction? transaction = null);
    
    /// <summary>
    /// Retrieves a specific invoice by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The invoice if found and active; otherwise, null.</returns>
    Task<Invoice?> GetInvoiceById(Guid id, DbTransaction? transaction = null);
    
    /// <summary>
    /// Retrieves all active invoices associated with a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>A collection of active invoices for the specified client.</returns>
    Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId, DbTransaction? transaction = null);
    
    /// <summary>
    /// Creates a new invoice in the database.
    /// </summary>
    /// <param name="invoice">The invoice information to create.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The created invoice with its assigned ID and creation timestamp.</returns>
    Task<Invoice> CreateInvoice(Invoice invoice, DbTransaction? transaction = null);
    
    /// <summary>
    /// Updates an existing invoice with new information.
    /// </summary>
    /// <param name="invoice">The invoice with updated information.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateInvoice(Invoice invoice, DbTransaction? transaction = null);
    
    /// <summary>
    /// Soft-deletes an invoice by marking it as deleted in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to delete.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
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

