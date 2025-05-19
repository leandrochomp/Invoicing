using Grpc.Core;
using NodaTime;

namespace InvoicingGrpc.Services;

public class ClientService : InvoicingGrpc.ClientService.ClientServiceBase
{
    private readonly ILogger<ClientService> _logger;
    private readonly IClock _clock;
    
    public ClientService(ILogger<ClientService> logger, IClock clock)
    {
        _logger = logger;
        _clock = clock;
    }

    public override Task<ClientResponse> CreateClient(CreateClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating new client: {ClientName}", request.Name);
        
        var now = _clock.GetCurrentInstant().ToString("o", null);
        
        // Here you would implement actual client creation logic using your data layer
        var response = new ClientResponse
        {
            ClientId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxId = request.TaxId,
            CreatedAt = now,
            UpdatedAt = now
        };
        
        return Task.FromResult(response);
    }

    public override Task<ClientResponse> GetClient(GetClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving client with ID: {ClientId}", request.ClientId);
        
        var now = _clock.GetCurrentInstant();
        var thirtyDaysAgo = now.Minus(Duration.FromDays(30)).ToString("o", null);
        
        // Here you would implement actual client retrieval logic
        var response = new ClientResponse
        {
            ClientId = request.ClientId,
            Name = "Sample Client",
            Email = "client@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            TaxId = "12345678901",
            CreatedAt = thirtyDaysAgo,
            UpdatedAt = now.ToString("o", null)
        };
        
        return Task.FromResult(response);
    }

    public override Task<ListClientsResponse> ListClients(ListClientsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Listing clients. Page: {Page}, Size: {Size}", 
            request.PageNumber, request.PageSize);
        
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
        var now = _clock.GetCurrentInstant();
        
        // Here you would implement actual client listing logic with pagination
        var response = new ListClientsResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 100 // Mock total count
        };
        
        // Add some mock clients
        for (var i = 0; i < pageSize; i++)
        {
            var daysAgo = now.Minus(Duration.FromDays(i));
            
            response.Clients.Add(new ClientResponse
            {
                ClientId = Guid.NewGuid().ToString(),
                Name = $"Client {i + 1}",
                Email = $"client{i+1}@example.com",
                Phone = "123-456-7890",
                Address = $"{i+1} Main St",
                TaxId = $"TAX{i+1}",
                CreatedAt = daysAgo.ToString("o", null),
                UpdatedAt = now.ToString("o", null)
            });
        }
        
        return Task.FromResult(response);
    }

    public override Task<ClientResponse> UpdateClient(UpdateClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Updating client with ID: {ClientId}", request.ClientId);
        
        var now = _clock.GetCurrentInstant().ToString("o", null);
        var thirtyDaysAgo = _clock.GetCurrentInstant().Minus(Duration.FromDays(30)).ToString("o", null);
        
        // Here you would implement actual client update logic
        var response = new ClientResponse
        {
            ClientId = request.ClientId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxId = request.TaxId,
            CreatedAt = thirtyDaysAgo,
            UpdatedAt = now
        };
        
        return Task.FromResult(response);
    }

    public override Task<DeleteClientResponse> DeleteClient(DeleteClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Deleting client with ID: {ClientId}", request.ClientId);
        
        // Here you would implement actual client deletion logic
        var response = new DeleteClientResponse
        {
            Success = true,
            Message = $"Client {request.ClientId} deleted successfully"
        };
        
        return Task.FromResult(response);
    }
}
