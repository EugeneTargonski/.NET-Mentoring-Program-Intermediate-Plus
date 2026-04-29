using Microsoft.AspNetCore.Mvc;
using Tickets.Services.Abstractions;

namespace Tickets.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet("{paymentId}")]
    public async Task<IActionResult> GetPaymentStatus(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var status = await paymentService.GetPaymentStatusAsync(paymentId, cancellationToken);
        return Ok(status);
    }

    [HttpPatch("{paymentId}/complete")]
    public async Task<IActionResult> CompletePayment(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var status = await paymentService.CompletePaymentAsync(paymentId, cancellationToken);
        return Ok(status);
    }

    [HttpPatch("{paymentId}/failed")]
    public async Task<IActionResult> FailPayment(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var status = await paymentService.FailPaymentAsync(paymentId, cancellationToken);
        return Ok(status);
    }
}
