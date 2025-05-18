using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Configuration.Extensions;

public static class SharedConfigurationExtensions
{
    public static IConfigurationBuilder AddSharedConfiguration(this IConfigurationBuilder builder, string? environmentName = null)
    {
        var sharedAssembly = typeof(SharedConfigurationExtensions).Assembly;
        var assemblyPath = Path.GetDirectoryName(sharedAssembly.Location);
        
        builder.SetBasePath(assemblyPath);
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        if (!string.IsNullOrEmpty(environmentName))
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
        }
        
        builder.AddEnvironmentVariables();
        
        return builder;
    }

    public static IServiceCollection AddSharedConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register database options
        services.AddDatabaseOptions(configuration);
        
        return services;
    }
}