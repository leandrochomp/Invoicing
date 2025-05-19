using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Configuration;

public class DatabaseOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Name { get; set; } = "invoicing";
    public string User { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";
    public int MinPoolSize { get; set; } = 1;
    public int MaxPoolSize { get; set; } = 20;
    public bool IncludeErrorDetail { get; set; } = true;
}

public static class DatabaseOptionsExtensions
{
    public static IServiceCollection AddDatabaseOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        return services;
    }
}