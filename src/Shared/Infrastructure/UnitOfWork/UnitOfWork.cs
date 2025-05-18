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
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(DbConnection connection)
    {
        _connection = connection;
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    public DbConnection Connection => _connection;
    public DbTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        _transaction = await _connection.BeginTransactionAsync(isolationLevel);
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
            _connection.Dispose();
        }

        _disposed = true;
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
