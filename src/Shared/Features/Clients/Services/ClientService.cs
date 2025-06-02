using Microsoft.Extensions.Logging;
using Shared.Features.Clients.Entities;
using Shared.Features.Clients.Repositories;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Clients.Services;

/// <summary>
/// Manages client CRUD operations of clients.
/// Provides the core functionality for client management in the invoicing system.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Retrieves all clients in the system.
    /// </summary>
    /// <returns>A collection of all clients.</returns>
    Task<IEnumerable<Client>> GetAllClients();
    
    /// <summary>
    /// Retrieves a specific client by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetClientById(Guid id);
    
    /// <summary>
    /// Creates a new client with the provided information.
    /// </summary>
    /// <param name="client">The client information to create.</param>
    /// <returns>The created client with its assigned ID.</returns>
    Task<Client> CreateClient(Client client);
    
    /// <summary>
    /// Updates an existing client with new information.
    /// </summary>
    /// <param name="client">The client with updated information.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateClient(Client client);
    
    /// <summary>
    /// Deletes a client by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
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
            var result = await _clientRepository.CreateClient(client);
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
            var result = await _clientRepository.UpdateClient(client);
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
            var result = await _clientRepository.DeleteClient(id);
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

