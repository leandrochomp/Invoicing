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

public class InvoiceService(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork) : IInvoiceService
{
    public async Task<IEnumerable<Invoice>> GetAllInvoices() => await invoiceRepository.GetAll();

    public async Task<Invoice?> GetInvoiceById(Guid id) => await invoiceRepository.GetInvoiceById(id);

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientId(Guid clientId)
    {
        return await invoiceRepository.GetInvoicesByClientId(clientId);
    }

    public async Task<Invoice> CreateInvoice(Invoice invoice)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var result = await invoiceRepository.CreateInvoice(invoice, unitOfWork.Transaction);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateInvoice(Invoice invoice)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var result = await invoiceRepository.UpdateInvoice(invoice, unitOfWork.Transaction);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteInvoice(Guid id)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var result = await invoiceRepository.DeleteInvoice(id, unitOfWork.Transaction);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateInvoiceStatus(Guid id, InvoiceStatus status)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var invoice = await invoiceRepository.GetInvoiceById(id, unitOfWork.Transaction);
            if (invoice == null)
            {
                return false;
            }

            invoice.Status = status;
            var result = await invoiceRepository.UpdateInvoice(invoice, unitOfWork.Transaction);

            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}