using Grpc.Core;
using NodaTime;
using Shared.Features.Clients.Entities;
using Shared.Features.Clients.Services;

namespace InvoicingGrpc.Services;

public class ClientService : ClientProtoService.ClientProtoServiceBase
{
    private readonly ILogger<ClientService> _logger;
    private readonly IClock _clock;
    private readonly IClientService _clientService;
    
    public ClientService(
        ILogger<ClientService> logger, 
        IClock clock,
        IClientService clientService)
    {
        _logger = logger;
        _clock = clock;
        _clientService = clientService;
    }

    public override async Task<ClientResponse> CreateClient(CreateClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating new client: {ClientName}", request.Name);
        
        try
        {
            // Map from gRPC request to domain model
            var client = new Client
            {
                Name = request.Name,
                Email = request.Email,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                CompanyName = request.CompanyName
            };
            
            // Use the actual client service to create the client
            var createdClient = await _clientService.CreateClient(client);
            
            // Map from domain model to gRPC response
            return MapToClientResponse(createdClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client: {ClientName}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, $"Error creating client: {ex.Message}"));
        }
    }

    public override async Task<ClientResponse> GetClient(GetClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving client with ID: {ClientId}", request.ClientId);
        
        try
        {
            // Parse the client ID
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Use the actual client service to get the client
            var client = await _clientService.GetClientById(clientId);
            
            if (client == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.ClientId} not found"));
            }
            
            // Map from domain model to gRPC response
            return MapToClientResponse(client);
        }
        catch (RpcException)
        {
            // Rethrow RPC exceptions as they're already properly formatted
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client: {ClientId}", request.ClientId);
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving client: {ex.Message}"));
        }
    }

    public override async Task<ListClientsResponse> ListClients(ListClientsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Listing clients. Page: {Page}, Size: {Size}", 
            request.PageNumber, request.PageSize);
        
        try
        {
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;
            var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
            
            // Use the actual client service to get all clients
            var clients = await _clientService.GetAllClients();
            var totalCount = clients.Count();
            
            // Apply pagination (in a real implementation, this should be done at the database level)
            var pagedClients = clients
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            
            // Create the response
            var response = new ListClientsResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            // Map each client to a client response and add to the list
            foreach (var client in pagedClients)
            {
                response.Clients.Add(MapToClientResponse(client));
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing clients");
            throw new RpcException(new Status(StatusCode.Internal, $"Error listing clients: {ex.Message}"));
        }
    }

    public override async Task<ClientResponse> UpdateClient(UpdateClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Updating client with ID: {ClientId}", request.ClientId);
        
        try
        {
            // Parse the client ID
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Get the existing client first
            var existingClient = await _clientService.GetClientById(clientId);
            if (existingClient == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.ClientId} not found"));
            }
            
            // Update the client properties
            existingClient.Name = request.Name;
            existingClient.Email = request.Email;
            existingClient.Address = request.Address;
            existingClient.PhoneNumber = request.PhoneNumber;
            existingClient.CompanyName = request.CompanyName;
            
            // Use the actual client service to update the client
            var success = await _clientService.UpdateClient(existingClient);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update client"));
            }
            
            // Get the updated client to return in the response
            var updatedClient = await _clientService.GetClientById(clientId);
            
            return MapToClientResponse(updatedClient!);
        }
        catch (RpcException)
        {
            // Rethrow RPC exceptions as they're already properly formatted
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client: {ClientId}", request.ClientId);
            throw new RpcException(new Status(StatusCode.Internal, $"Error updating client: {ex.Message}"));
        }
    }

    public override async Task<DeleteClientResponse> DeleteClient(DeleteClientRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Deleting client with ID: {ClientId}", request.ClientId);
        
        try
        {
            // Parse the client ID
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Use the actual client service to delete the client
            var success = await _clientService.DeleteClient(clientId);
            
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.ClientId} not found or could not be deleted"));
            }
            
            return new DeleteClientResponse
            {
                Success = true,
                Message = $"Client {request.ClientId} deleted successfully"
            };
        }
        catch (RpcException)
        {
            // Rethrow RPC exceptions as they're already properly formatted
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client: {ClientId}", request.ClientId);
            throw new RpcException(new Status(StatusCode.Internal, $"Error deleting client: {ex.Message}"));
        }
    }
    
    // Helper method to map from domain model to gRPC response
    private ClientResponse MapToClientResponse(Client client)
    {
        return new ClientResponse
        {
            ClientId = client.Id.ToString(),
            Name = client.Name,
            Email = client.Email,
            Address = client.Address ?? string.Empty,
            PhoneNumber = client.PhoneNumber ?? string.Empty,
            CompanyName = client.CompanyName ?? string.Empty,
            CreatedAt = client.CreatedAt.ToString(),
            UpdatedAt = client.UpdatedAt?.ToString() ?? client.CreatedAt.ToString()
        };
    }
}
