using Dapper;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Infrastructure.Data
{
    public interface IDbContext
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);
        Task<int> ExecuteAsync(string sql, object? param = null);
    }

    public class DapperDbContext : IDbContext
    {
        private readonly IUnitOfWork _unitOfWork;

        public DapperDbContext(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            EnsureConnection();
            return await _unitOfWork.Connection!.QueryAsync<T>(sql, param, _unitOfWork.Transaction);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            EnsureConnection();
            return await _unitOfWork.Connection!.QueryFirstOrDefaultAsync<T>(sql, param, _unitOfWork.Transaction);
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            EnsureConnection();
            return await _unitOfWork.Connection!.ExecuteAsync(sql, param, _unitOfWork.Transaction);
        }

        private void EnsureConnection()
        {
            if (_unitOfWork.Connection == null)
                throw new InvalidOperationException("Database connection is not initialized");
        }
    }
}