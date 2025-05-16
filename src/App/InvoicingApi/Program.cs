using InvoicingApi.Configuration;
using InvoicingApi.Hosting.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLogging(builder.Configuration);
builder.Services.AddTelemetry(builder.Configuration);

// Add API services from configurator
builder.Services.AddApiServices();
builder.Services.AddDatabaseServices(builder.Configuration);

var app = builder.Build();

// Configure the API
app.ConfigureApi();

app.Run();