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
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>A collection of all active clients.</returns>
    Task<IEnumerable<Client>> GetAll(DbTransaction? transaction = null);
    
    /// <summary>
    /// Retrieves a specific client by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The client if found and active; otherwise, null.</returns>
    Task<Client?> GetClientById(Guid id, DbTransaction? transaction = null);
    
    /// <summary>
    /// Creates a new client in the database.
    /// </summary>
    /// <param name="client">The client information to create.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>The created client with its assigned ID and creation timestamp.</returns>
    Task<Client> CreateClient(Client client, DbTransaction? transaction = null);
    
    /// <summary>
    /// Updates an existing client with new information.
    /// </summary>
    /// <param name="client">The client with updated information.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateClient(Client client, DbTransaction? transaction = null);
    
    /// <summary>
    /// Soft-deletes a client by marking it as deleted in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the client to delete.</param>
    /// <param name="transaction">Optional database transaction for coordinating multiple operations.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteClient(Guid id, DbTransaction? transaction = null);
}

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _dbContext;

    public ClientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Client>> GetAll(DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Clients.Where(c => !c.IsDeleted).ToListAsync();
    }

    public async Task<Client?> GetClientById(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }

        return await _dbContext.Clients.Where(client => client.Id == id && !client.IsDeleted).FirstOrDefaultAsync();
    }
    
    public async Task<Client> CreateClient(Client client, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }
        
        client.CreatedAt = SystemClock.Instance.GetCurrentInstant();
        
        await _dbContext.Clients.AddAsync(client);
        await _dbContext.SaveChangesAsync();
        
        return client;
    }
    
    public async Task<bool> UpdateClient(Client client, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }
    
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
    
    public async Task<bool> DeleteClient(Guid id, DbTransaction? transaction = null)
    {
        if (transaction != null)
        {
            await _dbContext.Database.UseTransactionAsync(transaction);
        }
    
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

