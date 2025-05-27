using System.Reflection;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InvoicingGrpc.Hosting.Config;

public static class TelemetryConfig
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var telemetryConfig = configuration.GetSection("Telemetry");
        var serviceName = telemetryConfig["ServiceName"] ?? "InvoicingGrpc";
        var serviceVersion = telemetryConfig["ServiceVersion"] ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        var otlpEndpoint = telemetryConfig["OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                }))
            .WithTracing(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddSource(serviceName)
                .AddSource("*") // Add all activity sources
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        options.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }
}
