using Microsoft.Extensions.Logging;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Invoices.Services;

/// <summary>
/// Manages invoice CRUD operations of invoices.
/// Handles invoice status changes and maintains invoice data integrity.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Retrieves all invoices in the system.
    /// </summary>
    /// <returns>A collection of all invoices.</returns>
    Task<IEnumerable<Invoice>> GetAllInvoices();
    
    /// <summary>
    /// Retrieves a specific invoice by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <returns>The invoice if found; otherwise, null.</returns>
    Task<Invoice?> GetInvoiceById(Guid id);
    
    /// <summary>
    /// Retrieves all invoices associated with a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <returns>A collection of invoices for the specified client.</returns>
    Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId);
    
    /// <summary>
    /// Creates a new invoice with the provided information.
    /// </summary>
    /// <param name="invoice">The invoice information to create.</param>
    /// <returns>The created invoice with its assigned ID.</returns>
    Task<Invoice> CreateInvoice(Invoice invoice);
    
    /// <summary>
    /// Updates an existing invoice with new information.
    /// </summary>
    /// <param name="invoice">The invoice with updated information.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateInvoice(Invoice invoice);
    
    /// <summary>
    /// Deletes an invoice by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteInvoice(Guid id);
    
    /// <summary>
    /// Updates the status of an invoice.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice.</param>
    /// <param name="status">The new status to apply to the invoice.</param>
    /// <returns>True if the status update was successful; otherwise, false.</returns>
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
