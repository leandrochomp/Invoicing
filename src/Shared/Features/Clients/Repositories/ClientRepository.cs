using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Clients.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Clients.Repositories;

/// <summary>
/// Provides data access operations for clients including retrieval, creation, update, and deletion.
/// Handles database interactions and transaction management for client-related operations.
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Retrieves all active (non-deleted) clients.
    /// </summary>
    /// <returns>A collection of all active clients.</returns>
    Task<IEnumerable<Client>> GetAll();
    
    /// <summary>
    /// Retrieves a specific client by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client.</param>
    /// <returns>The client if found and active; otherwise, null.</returns>
    Task<Client?> GetClientById(Guid id);
    
    /// <summary>
    /// Creates a new client in the database.
    /// </summary>
    /// <param name="client">The client information to create.</param>
    /// <returns>The created client with its assigned ID and creation timestamp.</returns>
    Task<Client> CreateClient(Client client);
    
    /// <summary>
    /// Updates an existing client with new information.
    /// </summary>
    /// <param name="client">The client with updated information.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateClient(Client client);
    
    /// <summary>
    /// Soft-deletes a client by marking it as deleted in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the client to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteClient(Guid id);
}

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Client>> GetAll()
    {
        return await _dbContext.Clients.Where(c => !c.IsDeleted).ToListAsync();
    }

    public async Task<Client?> GetClientById(Guid id)
    {
        return await _dbContext.Clients.Where(client => client.Id == id && !client.IsDeleted).FirstOrDefaultAsync();
    }
    
    public async Task<Client> CreateClient(Client client)
    {
        client.CreatedAt = SystemClock.Instance.GetCurrentInstant();
        
        await _dbContext.Clients.AddAsync(client);
        await _dbContext.SaveChangesAsync();
        
        return client;
    }
    
    public async Task<bool> UpdateClient(Client client)
    {
        var existingClient = await _dbContext.Clients
            .Where(c => c.Id == client.Id && !c.IsDeleted)
            .FirstOrDefaultAsync();
    
        if (existingClient == null)
        {
            return false;
        }
    
        // Update properties
        existingClient.Name = client.Name;
        existingClient.Email = client.Email;
        existingClient.Address = client.Address;
        existingClient.PhoneNumber = client.PhoneNumber;
        existingClient.CompanyName = client.CompanyName;
        existingClient.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteClient(Guid id)
    {
        var client = await _dbContext.Clients
            .Where(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync();
    
        if (client == null)
        {
            return false;
        }
    
        // Soft delete
        client.IsDeleted = true;
        client.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    
        await _dbContext.SaveChangesAsync();
        return true;
    }
}

