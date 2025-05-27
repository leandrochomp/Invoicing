using Shared.Features.Clients.Entities;
using Shared.Features.Clients.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Clients.Services;

public interface IClientService
{
    Task<IEnumerable<Client>> GetAllClients();
    Task<Client?> GetClientById(Guid id);
    Task<Client> CreateClient(Client client);
    Task<bool> UpdateClient(Client client);
    Task<bool> DeleteClient(Guid id);
}

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IClientRepository clientRepository, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Client>> GetAllClients()
    {
        return await _clientRepository.GetAll();
    }

    public async Task<Client?> GetClientById(Guid id)
    {
        return await _clientRepository.GetClientById(id);
    }

    public async Task<Client> CreateClient(Client client)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.CreateClient(client, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateClient(Client client)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.UpdateClient(client, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteClient(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.DeleteClient(id, _unitOfWork.Transaction);
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