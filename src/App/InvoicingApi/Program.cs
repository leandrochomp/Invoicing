using InvoicingApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add API services from configurator
builder.Services.AddApiServices();

var app = builder.Build();

// Configure the API
app.ConfigureApi();

app.Run();