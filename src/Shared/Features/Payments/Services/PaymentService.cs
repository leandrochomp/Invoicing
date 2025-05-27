using NodaTime;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Features.Payments.Entities;
using Shared.Features.Payments.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Payments.Services;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllPayments();
    Task<Payment?> GetPaymentById(Guid id);
    Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId);
    Task<Payment> CreatePayment(Payment payment);
    Task<bool> UpdatePayment(Payment payment);
    Task<bool> DeletePayment(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository, 
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Payment>> GetAllPayments()
    {
        return await _paymentRepository.GetAll();
    }

    public async Task<Payment?> GetPaymentById(Guid id)
    {
        return await _paymentRepository.GetPaymentById(id);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceId(Guid invoiceId)
    {
        return await _paymentRepository.GetPaymentsByInvoiceId(invoiceId);
    }

    public async Task<Payment> CreatePayment(Payment payment)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get the invoice
            var invoice = await _invoiceRepository.GetInvoiceById(payment.InvoiceId, _unitOfWork.Transaction);
            if (invoice == null)
            {
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
            decimal totalPaid = payments.Sum(p => p.AmountPaid) + payment.AmountPaid;
            
            if (totalPaid >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (totalPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            
            await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdatePayment(Payment payment)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get original payment
            var originalPayment = await _paymentRepository.GetPaymentById(payment.Id, _unitOfWork.Transaction);
            if (originalPayment == null)
            {
                return false;
            }
            
            // Update the payment
            var result = await _paymentRepository.UpdatePayment(payment, _unitOfWork.Transaction);
            
            // Update invoice status based on payments
            var invoice = await _invoiceRepository.GetInvoiceById(payment.InvoiceId, _unitOfWork.Transaction);
            if (invoice != null)
            {
                var payments = await _paymentRepository.GetPaymentsByInvoiceId(invoice.Id, _unitOfWork.Transaction);
                decimal totalPaid = payments.Sum(p => p.AmountPaid);
                
                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                }
                else if (totalPaid > 0)
                {
                    invoice.Status = InvoiceStatus.PartiallyPaid;
                }
                else
                {
                    invoice.Status = InvoiceStatus.Sent;
                }
                
                await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            }
            
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeletePayment(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Get the payment to find its invoice
            var payment = await _paymentRepository.GetPaymentById(id, _unitOfWork.Transaction);
            if (payment == null)
            {
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
                
                await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            }
            
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}