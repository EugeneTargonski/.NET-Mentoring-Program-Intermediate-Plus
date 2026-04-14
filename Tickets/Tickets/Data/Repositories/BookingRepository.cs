using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Data.Repositories;

/// <summary>
/// Cosmos DB implementation of IBookingRepository
/// </summary>
public class BookingRepository(Container container, ILogger<CosmosRepository<Booking>> logger) : CosmosRepository<Booking>(container, logger), IBookingRepository
{
    public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting bookings for customer ID: {CustomerId}", customerId);
            }

            var bookings = await QueryAsync(
                b => true, // Get all bookings for this partition
                partitionKey: customerId,
                cancellationToken: cancellationToken);

            return bookings.OrderByDescending(b => b.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for customer ID: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting bookings for event ID: {EventId}", eventId);
            }

            // Cross-partition query (less efficient but necessary)
            var bookings = await QueryAsync(
                b => b.EventId == eventId,
                partitionKey: null,
                cancellationToken: cancellationToken);

            return bookings.OrderByDescending(b => b.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for event ID: {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> ConfirmBookingAsync(string bookingId, string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Confirming booking ID: {BookingId}", bookingId);
            }

            var booking = await GetByIdAsync(bookingId, customerId, cancellationToken);

            if (booking == null || booking.Status != BookingStatus.Pending)
            {
                _logger.LogWarning("Booking ID: {BookingId} is not in pending status", bookingId);
                return false;
            }

            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;

            await UpdateAsync(booking, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully confirmed booking ID: {BookingId}", bookingId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking ID: {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<bool> CancelBookingAsync(string bookingId, string customerId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Cancelling booking ID: {BookingId}", bookingId);
            }

            var booking = await GetByIdAsync(bookingId, customerId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking ID: {BookingId} not found", bookingId);
                return false;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = reason;

            await UpdateAsync(booking, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully cancelled booking ID: {BookingId}", bookingId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking ID: {BookingId}", bookingId);
            throw;
        }
    }
}