using NodaTime;
using Shared.Features.Clients.Repositories;
using Shared.Features.Clients.Services;
using Shared.Infrastructure.UnitOfWork;

namespace InvoicingGrpc.Configuration;

public static class ServiceConfigurator
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        // Add gRPC
        services.AddGrpc();
        services.AddSingleton<IClock>(SystemClock.Instance);
        
        services.AddScoped<IClientService, ClientService>();
        
        return services;
    }
    
    public static IServiceCollection AddGrpcRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
}
