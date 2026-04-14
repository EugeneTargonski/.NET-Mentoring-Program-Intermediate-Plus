using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Demonstrate Ticket Service operations (TicketDb)
/// </summary>
public class TicketDemoScenarios(IUnitOfWork unitOfWork, ILogger<TicketDemoScenarios> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<TicketDemoScenarios> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllAsync()
    {
        await GenerateTicketAsync();
    }

    private async Task GenerateTicketAsync()
    {
        _logger.LogInformation("--- Demo: Generate Ticket (TicketDb) ---");

        var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync("customer123");
        var paidBooking = bookings.FirstOrDefault(b => b.Status == BookingStatus.Paid);
        
        if (paidBooking == null)
        {
            _logger.LogWarning("No paid bookings found");
            return;
        }

        var ticket = new Ticket
        {
            BookingId = paidBooking.Id,
            CustomerId = paidBooking.CustomerId,
            EventId = paidBooking.EventId,
            TicketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{paidBooking.Id[0..6].ToUpper()}",
            QrCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            SentAt = DateTime.UtcNow,
            EventInfo = paidBooking.EventInfo,
            SeatInfo = paidBooking.SeatInfo
        };

        await _unitOfWork.Tickets.CreateAsync(ticket);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created ticket in TicketDb: {TicketNumber}", ticket.TicketNumber);
        }
    }
}