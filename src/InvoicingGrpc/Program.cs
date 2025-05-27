using InvoicingGrpc.Configuration;
using InvoicingGrpc.Hosting.Config;
using Shared.Infrastructure.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLogging(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration);

// Add gRPC services from configurator
builder.Services.AddGrpcServices();
builder.Services.AddGrpcRepositories();
builder.Services.AddDatabaseServices(builder.Configuration);

var app = builder.Build();

// Configure the gRPC server
app.ConfigureGrpc();

app.Run();
