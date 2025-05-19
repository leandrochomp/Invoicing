using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Shared.Infrastructure.Data;

public interface IConnectionStringProvider
{
    string GetConnectionString();
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public ConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;    
        }
        
        // Build connection string from environment variables
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _configuration["Database:Host"] ?? Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
            Port = int.TryParse(_configuration["Database:Port"] ?? Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 5432,
            Database = _configuration["Database:Name"] ?? Environment.GetEnvironmentVariable("DB_NAME") ?? "invoicing",
            Username = _configuration["Database:User"] ?? Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
            Password = _configuration["Database:Password"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres",
            Pooling = true,
            MinPoolSize = int.TryParse(_configuration["Database:MinPoolSize"] ?? Environment.GetEnvironmentVariable("DB_MIN_POOL_SIZE"), out var minPool) ? minPool : 1,
            MaxPoolSize = int.TryParse(_configuration["Database:MaxPoolSize"] ?? Environment.GetEnvironmentVariable("DB_MAX_POOL_SIZE"), out var maxPool) ? maxPool : 20,
            IncludeErrorDetail = !bool.TryParse(_configuration["Database:IncludeErrorDetail"] ?? Environment.GetEnvironmentVariable("DB_INCLUDE_ERROR_DETAIL"), out var includeErrorDetail) || includeErrorDetail
        };

        return builder.ConnectionString;
    }
}
