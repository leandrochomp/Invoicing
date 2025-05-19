using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace Shared.Infrastructure.Configuration.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register connection string provider
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        services.AddSingleton<IUnitOfWork, UnitOfWork.UnitOfWork>();
        
        // Configure and register DbContext with EF Core
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var connectionStringProvider = provider.GetRequiredService<IConnectionStringProvider>();
            
            options.UseNpgsql(connectionStringProvider.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseNodaTime());
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Register DbConnection using the provider
        services.AddSingleton<DbConnection>(sp =>
        {
            var provider = sp.GetRequiredService<IConnectionStringProvider>();
            return new Npgsql.NpgsqlConnection(provider.GetConnectionString());
        });

        return services;
    }
}

