using System.Data;
using System.Data.Common;
using Npgsql;

namespace Shared.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    DbConnection? Connection { get; }
    DbTransaction? Transaction { get; }
    Task<bool> Get(string connectionString, CancellationToken cancellationToken);
    Task<bool> Get(string connectionString);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
}

public class UnitOfWork : IUnitOfWork
{
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public DbConnection? Connection => _connection;
    public DbTransaction? Transaction => _transaction;
    
    public Task<bool> Get(string connectionString)
    {
        return Get(connectionString, CancellationToken.None);
    }
    public async Task<bool> Get(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            if (_connection != null)
            {
                return true;
            }

            _connection = await CreateConnection(connectionString, cancellationToken);
            _transaction = await _connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (_transaction == null)
        {
            return false;
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await _transaction.RollbackAsync(cancellationToken);
            return false;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;

            // Start a new transaction for subsequent operations if connection is still open
            if (_connection?.State == ConnectionState.Open)
            {
                _transaction = await _connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            }
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
            _connection?.Dispose();
        }

        _transaction = null;
        _connection = null;
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
    
        Dispose(false);
        GC.SuppressFinalize(this);
    }
    
    private async Task<DbConnection> CreateConnection(string connectionString, CancellationToken cancellationToken)
    {
        var sqlConnection = new NpgsqlConnection(connectionString);
        await sqlConnection.OpenAsync(cancellationToken);
        return sqlConnection;
    }
    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        if (_transaction != null)
            await _transaction.DisposeAsync();
        
        if (_connection != null)
            await _connection.DisposeAsync();
        
        _transaction = null;
        _connection = null;
        _disposed = true;
    }
}