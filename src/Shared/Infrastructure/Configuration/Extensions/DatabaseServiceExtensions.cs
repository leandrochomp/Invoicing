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
        
        // Configure and register DbContext with EF Core
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var connectionStringProvider = provider.GetRequiredService<IConnectionStringProvider>();
            
            options.UseNpgsql(connectionStringProvider.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseNodaTime());
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        
        // Register UnitOfWork with the AppDbContext, not generic DbContext
        services.AddScoped<IUnitOfWork>(provider => 
            new UnitOfWork.UnitOfWork(provider.GetRequiredService<AppDbContext>()));

        return services;
    }
}

