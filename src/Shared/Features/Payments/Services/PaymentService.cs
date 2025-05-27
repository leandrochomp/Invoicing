using Microsoft.Extensions.Logging;
using NodaTime;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Features.Payments.Entities;
using Shared.Features.Payments.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Payments.Services;

/// <summary>
/// Manages payment CRUD operations of payments.
/// Also handles payment processing logic and updating invoice statuses based on payments.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Retrieves all payments in the system.
    /// </summary>
    /// <returns>A collection of all payments.</returns>
    Task<IEnumerable<Payment>> GetAllPayments();
    
    /// <summary>
    /// Retrieves a specific payment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>The payment if found; otherwise, null.</returns>
    Task<Payment?> GetPaymentById(Guid id);
    
    /// <summary>
    /// Retrieves all payments associated with a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the invoice.</param>
    /// <returns>A collection of payments for the specified invoice.</returns>
    Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId);
    
    /// <summary>
    /// Creates a new payment and updates the related invoice status.
    /// </summary>
    /// <param name="payment">The payment information to create.</param>
    /// <returns>The created payment with its assigned ID.</returns>
    Task<Payment> CreatePayment(Payment payment);
    
    /// <summary>
    /// Updates an existing payment and recalculates related invoice status.
    /// </summary>
    /// <param name="payment">The payment with updated information.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdatePayment(Payment payment);
    
    /// <summary>
    /// Deletes a payment and updates the related invoice status.
    /// </summary>
    /// <param name="id">The unique identifier of the payment to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeletePayment(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository, 
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Payment>> GetAllPayments()
    {
        _logger.LogInformation("Getting all payments");
        return await _paymentRepository.GetAll();
    }

    public async Task<Payment?> GetPaymentById(Guid id)
    {
        _logger.LogInformation("Getting payment by ID: {PaymentId}", id);
        return await _paymentRepository.GetPaymentById(id);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId)
    {
        _logger.LogInformation("Getting payments by invoice ID: {InvoiceId}", invoiceId);
        return await _paymentRepository.GetPaymentsByInvoiceId(invoiceId);
    }

    public async Task<Payment> CreatePayment(Payment payment)
    {
        _logger.LogInformation("Creating payment for invoice: {InvoiceId} with amount: {Amount}", 
            payment.InvoiceId, payment.AmountPaid);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get the invoice
            var invoice = await _invoiceRepository.GetInvoiceById(payment.InvoiceId, _unitOfWork.Transaction);
            if (invoice == null)
            {
                _logger.LogError("Invoice with ID {InvoiceId} not found when creating payment", payment.InvoiceId);
                throw new InvalidOperationException($"Invoice with ID {payment.InvoiceId} not found");
            }
            
            // Set payment date if not provided
            if (payment.PaymentDate == default)
            {
                payment.PaymentDate = SystemClock.Instance.GetCurrentInstant();
            }
            
            // Create the payment
            var result = await _paymentRepository.CreatePayment(payment, _unitOfWork.Transaction);
            
            // Update invoice status based on payment
            var payments = await _paymentRepository.GetPaymentsByInvoiceId(invoice.Id, _unitOfWork.Transaction);
            var totalPaid = payments.Sum(p => p.AmountPaid) + payment.AmountPaid;
            
            if (totalPaid >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
                _logger.LogInformation("Invoice {InvoiceId} marked as Paid", invoice.Id);
            }
            else if (totalPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
                _logger.LogInformation("Invoice {InvoiceId} marked as PartiallyPaid", invoice.Id);
            }
            
            await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Payment {PaymentId} created successfully", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for invoice {InvoiceId}", payment.InvoiceId);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdatePayment(Payment payment)
    {
        _logger.LogInformation("Updating payment: {PaymentId}", payment.Id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get original payment
            var originalPayment = await _paymentRepository.GetPaymentById(payment.Id, _unitOfWork.Transaction);
            if (originalPayment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found when attempting to update", payment.Id);
                return false;
            }
            
            // Update the payment
            var result = await _paymentRepository.UpdatePayment(payment, _unitOfWork.Transaction);
            
            // Update invoice status based on payments
            var invoice = await _invoiceRepository.GetInvoiceById(payment.InvoiceId, _unitOfWork.Transaction);
            if (invoice != null)
            {
                var payments = await _paymentRepository.GetPaymentsByInvoiceId(invoice.Id, _unitOfWork.Transaction);
                var totalPaid = payments.Sum(p => p.AmountPaid);
                
                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                    _logger.LogInformation("Invoice {InvoiceId} marked as Paid", invoice.Id);
                }
                else if (totalPaid > 0)
                {
                    invoice.Status = InvoiceStatus.PartiallyPaid;
                    _logger.LogInformation("Invoice {InvoiceId} marked as PartiallyPaid", invoice.Id);
                }
                else
                {
                    invoice.Status = InvoiceStatus.Sent;
                    _logger.LogInformation("Invoice {InvoiceId} marked as Sent", invoice.Id);
                }
                
                await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            }
            
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Payment {PaymentId} updated successfully", payment.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", payment.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeletePayment(Guid id)
    {
        _logger.LogInformation("Deleting payment: {PaymentId}", id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get the payment to find its invoice
            var payment = await _paymentRepository.GetPaymentById(id, _unitOfWork.Transaction);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found when attempting to delete", id);
                return false;
            }
            
            var invoiceId = payment.InvoiceId;
            
            // Delete the payment
            var result = await _paymentRepository.DeletePayment(id, _unitOfWork.Transaction);
            
            // Update invoice status
            var invoice = await _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction);
            if (invoice != null)
            {
                var payments = await _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction);
                var totalPaid = payments.Where(p => !p.IsDeleted).Sum(p => p.AmountPaid);
                
                invoice.UpdateStatusBasedOnPayments(totalPaid);
                _logger.LogInformation("Invoice {InvoiceId} status updated after payment deletion", invoice.Id);
                
                await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            }
            
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Payment {PaymentId} deleted successfully", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}

