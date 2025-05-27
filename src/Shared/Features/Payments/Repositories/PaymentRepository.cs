using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Payments.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Payments.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAll(DbTransaction? transaction = null);
    Task<Payment?> GetPaymentById(Guid id, DbTransaction? transaction = null);
    Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId, DbTransaction? transaction = null);
    Task<Payment> CreatePayment(Payment payment, DbTransaction? transaction = null);
    Task<bool> UpdatePayment(Payment payment, DbTransaction? transaction = null);
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