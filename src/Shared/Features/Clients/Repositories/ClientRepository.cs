using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Shared.Features.Clients.Entities;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Features.Clients.Repositories;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAll(DbTransaction? transaction = null);
    Task<Client?> GetClientById(Guid id, DbTransaction? transaction = null);
    Task<Client> CreateClient(Client client, DbTransaction? transaction = null);
    Task<bool> UpdateClient(Client client, DbTransaction? transaction = null);
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