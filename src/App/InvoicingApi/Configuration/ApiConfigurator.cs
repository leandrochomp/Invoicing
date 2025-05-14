using FastEndpoints;
using Scalar.AspNetCore;

namespace InvoicingApi.Configuration;

public static class ApiConfigurator
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddOpenApi();
        
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