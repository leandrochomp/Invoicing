using System.Text.Encodings.Web;
using System.Text.Json;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace InvoicingGrpc.Hosting.Config;

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
                    otlpOptions.Endpoint = new Uri(configuration["Telemetry:OtlpEndpoint"] ?? "http://localhost:4317");
                });
        });
        
        return builder;
    }
}

