using Microsoft.Extensions.Logging;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Invoices.Services;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetAllInvoices();
    Task<Invoice?> GetInvoiceById(Guid id);
    Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId);
    Task<Invoice> CreateInvoice(Invoice invoice);
    Task<bool> UpdateInvoice(Invoice invoice);
    Task<bool> DeleteInvoice(Guid id);
    Task<bool> UpdateInvoiceStatus(Guid id, InvoiceStatus status);
}

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoiceRepository, 
        IUnitOfWork unitOfWork,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoices()
    {
        _logger.LogInformation("Getting all invoices");
        return await _invoiceRepository.GetAll();
    }

    public async Task<Invoice?> GetInvoiceById(Guid id)
    {
        _logger.LogInformation("Getting invoice by ID: {InvoiceId}", id);
        return await _invoiceRepository.GetInvoiceById(id);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId)
    {
        _logger.LogInformation("Getting invoices by client ID: {ClientId}", clientId);
        return await _invoiceRepository.GetInvoicesByClientId(clientId);
    }

    public async Task<Invoice> CreateInvoice(Invoice invoice)
    {
        _logger.LogInformation("Creating invoice for client: {ClientId} with total amount: {TotalAmount}", 
            invoice.ClientId, invoice.TotalAmount);
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var result = await _invoiceRepository.CreateInvoice(invoice, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Invoice {InvoiceId} created successfully", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for client {ClientId}", invoice.ClientId);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateInvoice(Invoice invoice)
    {
        _logger.LogInformation("Updating invoice: {InvoiceId}", invoice.Id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Invoice {InvoiceId} updated successfully", invoice.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", invoice.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteInvoice(Guid id)
    {
        _logger.LogInformation("Deleting invoice: {InvoiceId}", id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _invoiceRepository.DeleteInvoice(id, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Invoice {InvoiceId} deleted successfully", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateInvoiceStatus(Guid id, InvoiceStatus status)
    {
        _logger.LogInformation("Updating invoice {InvoiceId} status to {Status}", id, status);
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var invoice = await _invoiceRepository.GetInvoiceById(id, _unitOfWork.Transaction);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found when attempting to update status", id);
                return false;
            }

            invoice.Status = status;
            var result = await _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction);

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Invoice {InvoiceId} status updated to {Status} successfully", id, status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId} status to {Status}", id, status);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
