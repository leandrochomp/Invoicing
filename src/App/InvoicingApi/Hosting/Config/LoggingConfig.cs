using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InvoicingApi.Hosting.Config;

public static class LoggingConfig
{
    public static ILoggingBuilder AddLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.ClearProviders();
        builder.AddDebug();
        builder.AddEventSourceLogger();
        
        builder.AddJsonConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            options.UseUtcTimestamp = true;
            options.IncludeScopes = true;
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        });
        
        builder.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector())
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions => 
                {
                    otlpOptions.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                });
        });
        
        return builder;
    }
    
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["ServiceName"] ?? "InvoicingApi";
        var serviceVersion = configuration["ServiceVersion"] ?? "1.0.0";

        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion)
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector())
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }))
            .WithMetrics(builder => builder
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }));
                
        // Add ActivitySource for custom tracing
        services.AddSingleton(new ActivitySource(serviceName));
        
        return services;
    }
}