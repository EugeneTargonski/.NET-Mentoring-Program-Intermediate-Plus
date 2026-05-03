using Microsoft.AspNetCore.Mvc;
using Moq;
using Tickets.Controllers;
using Tickets.DTOs;
using Tickets.Services.Abstractions;
using Xunit;

namespace Tickets.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _controller = new PaymentsController(_mockPaymentService.Object);
    }

    [Fact]
    public async Task GetPaymentStatus_ReturnsOkResult_WithPaymentStatus()
    {
        // Arrange
        var paymentId = "payment-123";
        var expectedStatus = new PaymentStatusResponse(
            paymentId,
            "Pending",
            150m,
            DateTime.UtcNow,
            null);

        _mockPaymentService
            .Setup(s => s.GetPaymentStatusAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetPaymentStatus(paymentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<PaymentStatusResponse>(okResult.Value);
        Assert.Equal(paymentId, returnedStatus.PaymentId);
        Assert.Equal("Pending", returnedStatus.Status);
        Assert.Equal(150m, returnedStatus.Amount);
        _mockPaymentService.Verify(s => s.GetPaymentStatusAsync(paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentStatus_PassesCancellationToken_ToService()
    {
        // Arrange
        var paymentId = "payment-456";
        var cancellationToken = new CancellationToken();
        var expectedStatus = new PaymentStatusResponse(paymentId, "Completed", 100m, DateTime.UtcNow, null);

        _mockPaymentService
            .Setup(s => s.GetPaymentStatusAsync(paymentId, cancellationToken))
            .ReturnsAsync(expectedStatus);

        // Act
        await _controller.GetPaymentStatus(paymentId, cancellationToken);

        // Assert
        _mockPaymentService.Verify(s => s.GetPaymentStatusAsync(paymentId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetPaymentStatus_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var paymentId = "payment-123";

        _mockPaymentService
            .Setup(s => s.GetPaymentStatusAsync(paymentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Payment not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.GetPaymentStatus(paymentId, CancellationToken.None));
    }

    [Fact]
    public async Task CompletePayment_ReturnsOkResult_WithUpdatedStatus()
    {
        // Arrange
        var paymentId = "payment-123";
        var expectedStatus = new PaymentStatusResponse(
            paymentId,
            "Completed",
            150m,
            DateTime.UtcNow,
            null);

        _mockPaymentService
            .Setup(s => s.CompletePaymentAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.CompletePayment(paymentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<PaymentStatusResponse>(okResult.Value);
        Assert.Equal(paymentId, returnedStatus.PaymentId);
        Assert.Equal("Completed", returnedStatus.Status);
        _mockPaymentService.Verify(s => s.CompletePaymentAsync(paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompletePayment_PassesCancellationToken_ToService()
    {
        // Arrange
        var paymentId = "payment-789";
        var cancellationToken = new CancellationToken();
        var expectedStatus = new PaymentStatusResponse(paymentId, "Completed", 200m, DateTime.UtcNow, null);

        _mockPaymentService
            .Setup(s => s.CompletePaymentAsync(paymentId, cancellationToken))
            .ReturnsAsync(expectedStatus);

        // Act
        await _controller.CompletePayment(paymentId, cancellationToken);

        // Assert
        _mockPaymentService.Verify(s => s.CompletePaymentAsync(paymentId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CompletePayment_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var paymentId = "payment-123";

        _mockPaymentService
            .Setup(s => s.CompletePaymentAsync(paymentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment already completed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.CompletePayment(paymentId, CancellationToken.None));
    }

    [Fact]
    public async Task FailPayment_ReturnsOkResult_WithUpdatedStatus()
    {
        // Arrange
        var paymentId = "payment-123";
        var expectedStatus = new PaymentStatusResponse(
            paymentId,
            "Failed",
            150m,
            DateTime.UtcNow,
            null);

        _mockPaymentService
            .Setup(s => s.FailPaymentAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.FailPayment(paymentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<PaymentStatusResponse>(okResult.Value);
        Assert.Equal(paymentId, returnedStatus.PaymentId);
        Assert.Equal("Failed", returnedStatus.Status);
        _mockPaymentService.Verify(s => s.FailPaymentAsync(paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FailPayment_PassesCancellationToken_ToService()
    {
        // Arrange
        var paymentId = "payment-999";
        var cancellationToken = new CancellationToken();
        var expectedStatus = new PaymentStatusResponse(paymentId, "Failed", 75m, DateTime.UtcNow, null);

        _mockPaymentService
            .Setup(s => s.FailPaymentAsync(paymentId, cancellationToken))
            .ReturnsAsync(expectedStatus);

        // Act
        await _controller.FailPayment(paymentId, cancellationToken);

        // Assert
        _mockPaymentService.Verify(s => s.FailPaymentAsync(paymentId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task FailPayment_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var paymentId = "payment-123";

        _mockPaymentService
            .Setup(s => s.FailPaymentAsync(paymentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Payment not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.FailPayment(paymentId, CancellationToken.None));
    }

    [Fact]
    public async Task GetPaymentStatus_ReturnsCorrectStatusForCompletedPayment()
    {
        // Arrange
        var paymentId = "payment-completed";
        var expectedStatus = new PaymentStatusResponse(
            paymentId,
            "Completed",
            250m,
            DateTime.UtcNow.AddMinutes(-10),
            null);

        _mockPaymentService
            .Setup(s => s.GetPaymentStatusAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetPaymentStatus(paymentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<PaymentStatusResponse>(okResult.Value);
        Assert.Equal("Completed", returnedStatus.Status);
    }

    [Fact]
    public async Task GetPaymentStatus_ReturnsCorrectStatusForFailedPayment()
    {
        // Arrange
        var paymentId = "payment-failed";
        var expectedStatus = new PaymentStatusResponse(
            paymentId,
            "Failed",
            180m,
            DateTime.UtcNow.AddMinutes(-5),
            "Payment declined");

        _mockPaymentService
            .Setup(s => s.GetPaymentStatusAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetPaymentStatus(paymentId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStatus = Assert.IsType<PaymentStatusResponse>(okResult.Value);
        Assert.Equal("Failed", returnedStatus.Status);
    }
}
