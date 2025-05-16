using System.Data.Common;
using FastEndpoints;
using Scalar.AspNetCore;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.UnitOfWork;

namespace InvoicingApi.Configuration;

public static class ApiConfigurator
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddOpenApi();
        
        return services;
    }
    
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register connection string provider
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        
        // Register UnitOfWork and DbContext
        services.AddSingleton<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDbContext, DapperDbContext>();
        
        // Register DbConnection using the provider
        services.AddSingleton<DbConnection>(sp => 
        {
            var provider = sp.GetRequiredService<IConnectionStringProvider>();
            return new Npgsql.NpgsqlConnection(provider.GetConnectionString());
        });
        
        return services;
    }

    public static WebApplication ConfigureApi(this WebApplication app)
    {
        app.UseFastEndpoints(c => 
        {
            c.Serializer.Options.PropertyNamingPolicy = null;
        });
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(opt =>
            {
                opt
                    .WithTitle("Invoicing 1.0")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }
        
        app.UseHttpsRedirection();
        
        return app;
    }
}