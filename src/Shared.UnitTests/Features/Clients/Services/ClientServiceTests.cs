using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Shared.Features.Clients.Entities;
using Shared.Features.Clients.Repositories;
using Shared.Features.Clients.Services;
using Shared.Infrastructure.UnitOfWork;
using Shouldly;

namespace Shared.UnitTests.Features.Clients.Services;

public class ClientServiceTests
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        // Setup mocks
        _clientRepository = Substitute.For<IClientRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _clientService = new ClientService(_clientRepository, _unitOfWork);
    }

    [Fact]
    public async Task GetAllClients_ShouldReturnAllClients()
    {
        // Arrange
        var expectedClients = new List<Client>
        {
            new() { Id = Guid.NewGuid(), Name = "Client 1" },
            new() { Id = Guid.NewGuid(), Name = "Client 2" }
        };
        
        _clientRepository.GetAll().Returns(expectedClients);

        // Act
        var result = await _clientService.GetAllClients();

        // Assert
        result.ShouldBe(expectedClients);
        await _clientRepository.Received(1).GetAll();
    }

    [Fact]
    public async Task GetClientById_WithValidId_ShouldReturnClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expectedClient = new Client { Id = clientId, Name = "Client 1" };
        
        _clientRepository.GetClientById(clientId).Returns(expectedClient);

        // Act
        var result = await _clientService.GetClientById(clientId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedClient);
        await _clientRepository.Received(1).GetClientById(clientId);
    }

    [Fact]
    public async Task GetClientById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        
        _clientRepository.GetClientById(clientId).ReturnsNull();

        // Act
        var result = await _clientService.GetClientById(clientId);

        // Assert
        result.ShouldBeNull();
        await _clientRepository.Received(1).GetClientById(clientId);
    }

    [Fact]
    public async Task CreateClient_ShouldCreateAndReturnClient()
    {
        // Arrange
        var client = new Client
        {
            Name = "New Client",
            Email = "client@example.com"
        };
        
        var savedClient = new Client
        {
            Id = Guid.NewGuid(),
            Name = client.Name,
            Email = client.Email
        };
        
        _clientRepository.CreateClient(client, _unitOfWork.Transaction).Returns(savedClient);

        // Act
        var result = await _clientService.CreateClient(client);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(savedClient);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _clientRepository.Received(1).CreateClient(client, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CreateClient_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        // Arrange
        var client = new Client
        {
            Name = "New Client",
            Email = "client@example.com"
        };
        
        var expectedException = new Exception("Database error");
        _clientRepository.CreateClient(client, _unitOfWork.Transaction).Throws(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _clientService.CreateClient(client));
        exception.ShouldBe(expectedException);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task UpdateClient_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Updated Client",
            Email = "updated@example.com"
        };
        
        _clientRepository.UpdateClient(client, _unitOfWork.Transaction).Returns(true);

        // Act
        var result = await _clientService.UpdateClient(client);

        // Assert
        result.ShouldBeTrue();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _clientRepository.Received(1).UpdateClient(client, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task UpdateClient_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        // Arrange
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Updated Client",
            Email = "updated@example.com"
        };
        
        var expectedException = new Exception("Database error");
        _clientRepository.UpdateClient(client, _unitOfWork.Transaction).Throws(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _clientService.UpdateClient(client));
        exception.ShouldBe(expectedException);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task DeleteClient_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        
        _clientRepository.DeleteClient(clientId, _unitOfWork.Transaction).Returns(true);

        // Act
        var result = await _clientService.DeleteClient(clientId);

        // Assert
        result.ShouldBeTrue();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _clientRepository.Received(1).DeleteClient(clientId, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteClient_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        
        var expectedException = new Exception("Database error");
        _clientRepository.DeleteClient(clientId, _unitOfWork.Transaction).Throws(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _clientService.DeleteClient(clientId));
        exception.ShouldBe(expectedException);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }
}
