using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace Shared.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    DbConnection Connection { get; }
    DbTransaction? Transaction { get; }
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private DbTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DbConnection Connection => _dbContext.Database.GetDbConnection();
    public DbTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync();
        }

        _transaction = await Connection.BeginTransactionAsync(isolationLevel);
        
        await _dbContext.Database.UseTransactionAsync(_transaction);
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _transaction?.Dispose();
            // Don't dispose the connection here as it's owned by DbContext
        }

        _disposed = true;
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
