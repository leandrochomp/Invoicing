using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clientRepository, 
        IUnitOfWork unitOfWork,
        ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Client>> GetAllClients()
    {
        _logger.LogInformation("Getting all clients");
        return await _clientRepository.GetAll();
    }

    public async Task<Client?> GetClientById(Guid id)
    {
        _logger.LogInformation("Getting client by ID: {ClientId}", id);
        return await _clientRepository.GetClientById(id);
    }

    public async Task<Client> CreateClient(Client client)
    {
        _logger.LogInformation("Creating client: {ClientName}", client.Name);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.CreateClient(client, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Client {ClientId} created successfully", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client {ClientName}", client.Name);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateClient(Client client)
    {
        _logger.LogInformation("Updating client: {ClientId}", client.Id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.UpdateClient(client, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Client {ClientId} updated successfully", client.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client {ClientId}", client.Id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteClient(Guid id)
    {
        _logger.LogInformation("Deleting client: {ClientId}", id);
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var result = await _clientRepository.DeleteClient(id, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Client {ClientId} deleted successfully", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client {ClientId}", id);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}

