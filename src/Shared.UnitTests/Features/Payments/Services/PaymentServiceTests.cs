using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shared.Features.Invoices.Entities;
using Shared.Features.Invoices.Repositories;
using Shared.Features.Payments.Entities;
using Shared.Features.Payments.Repositories;
using Shared.Features.Payments.Services;
using Shared.Infrastructure.UnitOfWork;
using Shouldly;

namespace Shared.UnitTests.Features.Payments.Services;

public class PaymentServiceTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        // Setup mocks
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _invoiceRepository = Substitute.For<IInvoiceRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<PaymentService>>();
        _paymentService = new PaymentService(_paymentRepository, _invoiceRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task GetAllPayments_ShouldReturnAllPayments()
    {
        // Arrange
        var expectedPayments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), InvoiceId = Guid.NewGuid(), AmountPaid = 100m },
            new() { Id = Guid.NewGuid(), InvoiceId = Guid.NewGuid(), AmountPaid = 200m }
        };
        
        _paymentRepository.GetAll().Returns(expectedPayments);

        // Act
        var result = await _paymentService.GetAllPayments();

        // Assert
        result.ShouldBe(expectedPayments);
        await _paymentRepository.Received(1).GetAll();
    }

    [Fact]
    public async Task GetPaymentById_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var expectedPayment = new Payment { Id = paymentId, InvoiceId = Guid.NewGuid(), AmountPaid = 100m };
        
        _paymentRepository.GetPaymentById(paymentId).Returns(expectedPayment);

        // Act
        var result = await _paymentService.GetPaymentById(paymentId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedPayment);
        await _paymentRepository.Received(1).GetPaymentById(paymentId);
    }

    [Fact]
    public async Task GetPaymentById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentRepository.GetPaymentById(paymentId).ReturnsNull();

        // Act
        var result = await _paymentService.GetPaymentById(paymentId);

        // Assert
        result.ShouldBeNull();
        await _paymentRepository.Received(1).GetPaymentById(paymentId);
    }

    [Fact]
    public async Task GetPaymentsByInvoiceId_ShouldReturnInvoicePayments()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var expectedPayments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), InvoiceId = invoiceId, AmountPaid = 50m },
            new() { Id = Guid.NewGuid(), InvoiceId = invoiceId, AmountPaid = 100m }
        };
        
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId).Returns(expectedPayments);

        // Act
        var result = await _paymentService.GetPaymentsByInvoiceId(invoiceId);

        // Assert
        result.ShouldBe(expectedPayments);
        await _paymentRepository.Received(1).GetPaymentsByInvoiceId(invoiceId);
    }

    [Fact]
    public async Task CreatePayment_ShouldCreateAndReturnPaymentAndUpdateInvoiceStatus()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            TotalAmount = 200m,
            Status = InvoiceStatus.Sent
        };
        
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        var existingPayments = new List<Payment>();
        
        var savedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            AmountPaid = 100m,
            PaymentDate = SystemClock.Instance.GetCurrentInstant()
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(invoice);
        _paymentRepository.CreatePayment(payment, _unitOfWork.Transaction).Returns(savedPayment);
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction).Returns(existingPayments);

        // Act
        var result = await _paymentService.CreatePayment(payment);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(savedPayment);
        
        // Verify the payment date was set
        payment.PaymentDate.ShouldNotBe(default);
        
        // Verify invoice status was updated to PartiallyPaid
        await _invoiceRepository.Received(1).UpdateInvoice(
            Arg.Is<Invoice>(i => i.Status == InvoiceStatus.PartiallyPaid), 
            _unitOfWork.Transaction);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _paymentRepository.Received(1).CreatePayment(payment, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CreatePayment_WithFullAmount_ShouldUpdateInvoiceStatusToPaid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            TotalAmount = 200m,
            Status = InvoiceStatus.Sent
        };
        
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = 200m
        };
        
        var existingPayments = new List<Payment>();
        
        var savedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            AmountPaid = 200m,
            PaymentDate = SystemClock.Instance.GetCurrentInstant()
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(invoice);
        _paymentRepository.CreatePayment(payment, _unitOfWork.Transaction).Returns(savedPayment);
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction).Returns(existingPayments);

        // Act
        var result = await _paymentService.CreatePayment(payment);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(savedPayment);
        
        // Verify invoice status was updated to Paid
        await _invoiceRepository.Received(1).UpdateInvoice(
            Arg.Is<Invoice>(i => i.Status == InvoiceStatus.Paid), 
            _unitOfWork.Transaction);
    }

    [Fact]
    public async Task CreatePayment_WithInvalidInvoiceId_ShouldThrowException()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).ReturnsNull();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _paymentService.CreatePayment(payment));
        
        exception.Message.ShouldContain(invoiceId.ToString());
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _unitOfWork.Received(1).RollbackAsync();
        await _paymentRepository.DidNotReceive().CreatePayment(Arg.Any<Payment>(), _unitOfWork.Transaction);
    }

    [Fact]
    public async Task UpdatePayment_ShouldUpdateAndRecalculateInvoiceStatus()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        
        var invoice = new Invoice
        {
            Id = invoiceId,
            TotalAmount = 200m,
            Status = InvoiceStatus.PartiallyPaid
        };
        
        var originalPayment = new Payment
        {
            Id = paymentId,
            InvoiceId = invoiceId,
            AmountPaid = 50m
        };
        
        var updatedPayment = new Payment
        {
            Id = paymentId,
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        var otherPayment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        var payments = new List<Payment> { updatedPayment, otherPayment };
        
        _paymentRepository.GetPaymentById(paymentId, _unitOfWork.Transaction).Returns(originalPayment);
        _paymentRepository.UpdatePayment(updatedPayment, _unitOfWork.Transaction).Returns(true);
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(invoice);
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction).Returns(payments);

        // Act
        var result = await _paymentService.UpdatePayment(updatedPayment);

        // Assert
        result.ShouldBeTrue();
        
        // Verify invoice status was updated to Paid (since total payments = 200m)
        await _invoiceRepository.Received(1).UpdateInvoice(
            Arg.Is<Invoice>(i => i.Status == InvoiceStatus.Paid), 
            _unitOfWork.Transaction);
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _paymentRepository.Received(1).UpdatePayment(updatedPayment, _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeletePayment_ShouldDeleteAndRecalculateInvoiceStatus()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        
        var invoice = new Invoice
        {
            Id = invoiceId,
            TotalAmount = 200m,
            Status = InvoiceStatus.Paid
        };
        
        var paymentToDelete = new Payment
        {
            Id = paymentId,
            InvoiceId = invoiceId,
            AmountPaid = 150m
        };
        
        var remainingPayment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            AmountPaid = 50m,
            IsDeleted = false
        };
        
        var remainingPayments = new List<Payment> { remainingPayment };
        
        _paymentRepository.GetPaymentById(paymentId, _unitOfWork.Transaction).Returns(paymentToDelete);
        _paymentRepository.DeletePayment(paymentId, _unitOfWork.Transaction).Returns(true);
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(invoice);
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction).Returns(remainingPayments);

        // Act
        var result = await _paymentService.DeletePayment(paymentId);

        // Assert
        result.ShouldBeTrue();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _paymentRepository.Received(1).DeletePayment(paymentId, _unitOfWork.Transaction);
        await _invoiceRepository.Received(1).UpdateInvoice(Arg.Any<Invoice>(), _unitOfWork.Transaction);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeletePayment_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        
        _paymentRepository.GetPaymentById(paymentId, _unitOfWork.Transaction).ReturnsNull();

        // Act
        var result = await _paymentService.DeletePayment(paymentId);

        // Assert
        result.ShouldBeFalse();
        
        // Verify transaction flow
        await _unitOfWork.Received(1).BeginTransactionAsync();
        await _paymentRepository.DidNotReceive().DeletePayment(Arg.Any<Guid>(), _unitOfWork.Transaction);
        await _unitOfWork.Received(0).CommitAsync();
    }

    [Fact]
    public async Task CreatePayment_ShouldLogAppropriateMessages()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            TotalAmount = 200m,
            Status = InvoiceStatus.Sent
        };
        
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        var existingPayments = new List<Payment>();
        
        var savedPayment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            AmountPaid = 100m,
            PaymentDate = SystemClock.Instance.GetCurrentInstant()
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).Returns(invoice);
        _paymentRepository.CreatePayment(payment, _unitOfWork.Transaction).Returns(savedPayment);
        _paymentRepository.GetPaymentsByInvoiceId(invoiceId, _unitOfWork.Transaction).Returns(existingPayments);

        // Act
        var result = await _paymentService.CreatePayment(payment);

        // Assert
        // Verify logging messages were called
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Creating payment for invoice: {invoiceId}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
        
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Invoice {invoiceId} marked as PartiallyPaid")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
        
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Payment {savedPayment.Id} created successfully")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }

    [Fact]
    public async Task CreatePayment_WithInvalidInvoiceId_ShouldLogError()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AmountPaid = 100m
        };
        
        _invoiceRepository.GetInvoiceById(invoiceId, _unitOfWork.Transaction).ReturnsNull();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _paymentService.CreatePayment(payment));
        
        // Verify error logging occurred
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Invoice with ID {invoiceId} not found")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }
}
