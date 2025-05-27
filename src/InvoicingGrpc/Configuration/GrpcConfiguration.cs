namespace InvoicingGrpc.Configuration;

public static class GrpcConfiguration
{
    public static WebApplication ConfigureGrpc(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        
        // Register the ClientProtoService - This exposes the following gRPC methods:
        // - CreateClient: Creates a new client
        // - GetClient: Retrieves a client by ID
        // - ListClients: Lists clients with pagination
        // - UpdateClient: Updates an existing client
        // - DeleteClient: Deletes a client by ID
        app.MapGrpcService<Services.ClientService>();
        
        // Add health checks endpoint
        app.MapGet("/health", () => Results.Ok("Healthy"));
        
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        
        return app;
    }
}
