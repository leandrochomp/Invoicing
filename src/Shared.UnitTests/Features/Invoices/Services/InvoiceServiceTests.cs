using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Features.Invoices.Services;
using Shared.Infrastructure.UnitOfWork;
using Shouldly;

namespace Shared.UnitTests.Features.Invoices.Services;

public class InvoiceServiceTests
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        // Setup mocks
        _invoiceRepository = Substitute.For<IInvoiceRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _invoiceService = new InvoiceService(_invoiceRepository, _unitOfWork);
    }

    [Fact]
    public async Task GetAllInvoices_ShouldReturnAllInvoices()
    {
        // Arrange
        var expectedInvoices = new List<Invoice>
        {
            new() { Id = Guid.NewGuid(), InvoiceNumber = "INV-001" },
            new() { Id = Guid.NewGuid(), InvoiceNumber = "INV-002" }
        };
        
        _invoiceRepository.GetAll().Returns(expectedInvoices);

        // Act
        var result = await _invoiceService.GetAllInvoices();

        // Assert
        result.ShouldBe(expectedInvoices);
        await _invoiceRepository.Received(1).GetAll();
    }

    [Fact]
    public async Task GetInvoiceById_WithValidId_ShouldReturnInvoice()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var expectedInvoice = new Invoice { Id = invoiceId, InvoiceNumber = "INV-001" };
        
        _invoiceRepository.GetInvoiceById(invoiceId).Returns(expectedInvoice);

        // Act
        var result = await _invoiceService.GetInvoiceById(invoiceId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedInvoice);
        await _invoiceRepository.Received(1).GetInvoiceById(invoiceId);
    }

    [Fact]
    public async Task GetInvoiceById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        
        _invoiceRepository.GetInvoiceById(invoiceId).ReturnsNull();

        // Act
        var result = await _invoiceService.GetInvoiceById(invoiceId);

        // Assert
        result.ShouldBeNull();
        await _invoiceRepository.Received(1).GetInvoiceById(invoiceId);
    }

    [Fact]
    public async Task GetInvoicesByClientId_ShouldReturnClientInvoices()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expectedInvoices = new List<Invoice>
        {
            new() { Id = Guid.NewGuid(), ClientId = clientId, InvoiceNumber = "INV-001" },
            new() { Id = Guid.NewGuid(), ClientId = clientId, InvoiceNumber = "INV-002" }
        };
        
        _invoiceRepository.GetInvoicesByClientId(clientId).Returns(expectedInvoices);

        // Act
        var result = await _invoiceService.GetInvoicesByClientId(clientId);

        // Assert
        result.ShouldBe(expectedInvoices);
        await _invoiceRepository.Received(1).GetInvoicesByClientId(clientId);
    }

    [Fact]
    public async Task CreateInvoice_ShouldCreateAndReturnInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            TotalAmount = 100.0m
        };
        
        var savedInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = invoice.ClientId,
            InvoiceNumber = invoice.InvoiceNumber,
            TotalAmount = invoice.TotalAmount
        };
        
        _invoiceRepository.CreateInvoice(invoice, _unitOfWork.Transaction).Returns(savedInvoice);

        // Act
        var result = await _invoiceService.CreateInvoice(invoice);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(savedInvoice);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _invoiceRepository.Received(1).CreateInvoice(invoice, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CreateInvoice_WhenRepositoryThrowsException_ShouldRollbackAndRethrow()
    {
        // Arrange
        var invoice = new Invoice
        {
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            TotalAmount = 100.0m
        };
        
        var expectedException = new Exception("Database error");
        _invoiceRepository.CreateInvoice(invoice, _unitOfWork.Transaction).Throws(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(() => _invoiceService.CreateInvoice(invoice));
        exception.ShouldBe(expectedException);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task UpdateInvoice_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            InvoiceNumber = "INV-001",
            TotalAmount = 150.0m
        };
        
        _invoiceRepository.UpdateInvoice(invoice, _unitOfWork.Transaction).Returns(true);

        // Act
        var result = await _invoiceService.UpdateInvoice(invoice);

        // Assert
        result.ShouldBeTrue();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _invoiceRepository.Received(1).UpdateInvoice(invoice, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteInvoice_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        
        _invoiceRepository.DeleteInvoice(invoiceId, _unitOfWork.Transaction).Returns(true);

        // Act
        var result = await _invoiceService.DeleteInvoice(invoiceId);

        // Assert
        result.ShouldBeTrue();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _invoiceRepository.Received(1).DeleteInvoice(invoiceId, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task UpdateInvoiceStatus_WithValidId_ShouldUpdateStatusAndReturnSuccess()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var newStatus = InvoiceStatus.Paid;
        
        var existingInvoice = new Invoice
        {
            Id = invoiceId,
            Status = InvoiceStatus.Sent
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(existingInvoice);
        _invoiceRepository.UpdateInvoice(Arg.Any<Invoice>(), _unitOfWork.Transaction).Returns(true);

        // Act
        var result = await _invoiceService.UpdateInvoiceStatus(invoiceId, newStatus);

        // Assert
        result.ShouldBeTrue();
        
        // Verify the invoice was updated with the new status
        await _invoiceRepository.Received(1).UpdateInvoice(
            Arg.Is<Invoice>(i => i.Status == newStatus), 
            _unitOfWork.Transaction);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task UpdateInvoiceStatus_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var newStatus = InvoiceStatus.Paid;
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).ReturnsNull();

        // Act
        var result = await _invoiceService.UpdateInvoiceStatus(invoiceId, newStatus);

        // Assert
        result.ShouldBeFalse();
        
        // Verify invoice update was not attempted
        await _invoiceRepository.DidNotReceive().UpdateInvoice(Arg.Any<Invoice>(), _unitOfWork.Transaction);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(0).CommitAsync();
    }
}
