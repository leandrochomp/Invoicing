using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Payments.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Payments.Repositories;

/// <summary>
/// Provides data access operations for payments including retrieval, creation, update, and deletion.
/// Handles database interactions and transaction management for payment-related operations.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Retrieves all active (non-deleted) payments.
    /// </summary>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>A collection of all active payments.</returns>
    Task<IEnumerable<Payment>> GetAll(DbTransaction? transaction = null);
    
    /// <summary>
    /// Retrieves a specific payment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The payment if found and active; otherwise, null.</returns>
    Task<Payment?> GetPaymentById(Guid id, DbTransaction? transaction = null);
    
    /// <summary>
    /// Retrieves all active payments associated with a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>A collection of active payments for the specified invoice.</returns>
    Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId, DbTransaction? transaction = null);
    
    /// <summary>
    /// Creates a new payment in the database.
    /// </summary>
    /// <param name="payment">The payment information to create.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The created payment with its assigned ID and creation timestamp.</returns>
    Task<Payment> CreatePayment(Payment payment, DbTransaction? transaction = null);
    
    /// <summary>
    /// Updates an existing payment with new information.
    /// </summary>
    /// <param name="payment">The payment with updated information.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdatePayment(Payment payment, DbTransaction? transaction = null);
    
    /// <summary>
    /// Soft-deletes a payment by marking it as deleted in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the payment to delete.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeletePayment(Guid id, DbTransaction? transaction = null);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _dbContext;

    public PaymentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Payment>> GetAll(DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Payments.Where(p => !p.IsDeleted).ToListAsync();
    }

    public async Task<Payment?> GetPaymentById(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Payments.Where(p => p.Id == id && !p.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            _dbContext.Database.UseTransaction(transaction);
        }

        return await _dbContext.Payments.Where(p => p.InvoiceId == invoiceId && !p.IsDeleted).ToListAsync();
    }

    public async Task<Payment> CreatePayment(Payment payment, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        // Ensure proper timestamps are set
        payment.CreatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.Payments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();

        return payment;
    }

    public async Task<bool> UpdatePayment(Payment payment, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        var existingPayment = await _dbContext.Payments
            .Where(p => p.Id == payment.Id && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (existingPayment == null)
        {
            return false;
        }

        // Update properties
        existingPayment.InvoiceId = payment.InvoiceId;
        existingPayment.AmountPaid = payment.AmountPaid;
        existingPayment.PaymentDate = payment.PaymentDate;
        existingPayment.PaymentMethod = payment.PaymentMethod;
        existingPayment.ReferenceNumber = payment.ReferenceNumber;
        existingPayment.Notes = payment.Notes;
        existingPayment.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePayment(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        var payment = await _dbContext.Payments
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return false;
        }

        // Soft delete
        payment.IsDeleted = true;
        payment.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        await _dbContext.SaveChangesAsync();
        return true;
    }
}

