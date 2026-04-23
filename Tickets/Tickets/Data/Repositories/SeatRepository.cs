using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Data.Repositories;

/// <summary>
/// Cosmos DB implementation of ISeatRepository
/// </summary>
public class SeatRepository(Container container, ILogger<CosmosRepository<Seat>> logger) : CosmosRepository<Seat>(container, logger), ISeatRepository
{
    public async Task<IEnumerable<Seat>> GetAvailableSeatsByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting available seats for event ID: {EventId}", eventId);
            }

            var seats = await QueryAsync(
                s => s.Status == SeatStatus.Available,
                partitionKey: eventId,
                cancellationToken: cancellationToken);

            return seats.OrderBy(s => s.Section)
                       .ThenBy(s => s.Row)
                       .ThenBy(s => s.SeatNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available seats for event ID: {EventId}", eventId);
            throw;
        }
    }

    public async Task<IEnumerable<Seat>> GetAvailableSeatsWithOffersByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting available seats with offers for event ID: {EventId}", eventId);
            }

            var seats = await QueryAsync(
                s => s.Status == SeatStatus.Available && s.CurrentOffer != null,
                partitionKey: eventId,
                cancellationToken: cancellationToken);

            // Sort by price (lowest first) - offers are embedded
            return seats.OrderBy(s => s.CurrentOffer!.Price)
                       .ThenBy(s => s.Section)
                       .ThenBy(s => s.Row);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available seats with offers for event ID: {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> HoldSeatAsync(string seatId, string eventId, string customerId, DateTime holdExpiresAt, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Holding seat ID: {SeatId} for customer: {CustomerId}", seatId, customerId);
            }

            var seat = await GetByIdAsync(seatId, eventId, cancellationToken);

            if (seat == null || seat.Status != SeatStatus.Available)
            {
                _logger.LogWarning("Seat ID: {SeatId} is not available", seatId);
                return false;
            }

            // Optimistic concurrency control using ETag
            seat.Status = SeatStatus.OnHold;
            seat.HeldByCustomerId = customerId;
            seat.HoldExpiresAt = holdExpiresAt;

            await UpdateAsync(seat, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully held seat ID: {SeatId}", seatId);
            }

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            _logger.LogWarning("Concurrency conflict while holding seat ID: {SeatId}", seatId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error holding seat ID: {SeatId}", seatId);
            throw;
        }
    }

    public async Task<int> ReleaseExpiredHoldsAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Releasing expired seat holds for event ID: {EventId}", eventId);
            }

            var expiredSeats = await QueryAsync(
                s => s.Status == SeatStatus.OnHold && s.HoldExpiresAt < DateTime.UtcNow,
                partitionKey: eventId,
                cancellationToken: cancellationToken);
            
            var expiredList = expiredSeats.ToList();

            foreach (var seat in expiredList)
            {
                seat.Status = SeatStatus.Available;
                seat.HeldByCustomerId = null;
                seat.HoldExpiresAt = null;
                await UpdateAsync(seat, cancellationToken);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Released {Count} expired seat holds for event ID: {EventId}", expiredList.Count, eventId);
            }

            return expiredList.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing expired seat holds for event ID: {EventId}", eventId);
            throw;
        }
    }

    public async Task<bool> ReserveSeatAsync(string seatId, string eventId, string bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Reserving seat ID: {SeatId} for booking: {BookingId}", seatId, bookingId);
            }

            var seat = await GetByIdAsync(seatId, eventId, cancellationToken);

            if (seat == null || seat.Status != SeatStatus.OnHold)
            {
                _logger.LogWarning("Seat ID: {SeatId} is not in OnHold status", seatId);
                return false;
            }

            seat.Status = SeatStatus.Booked;
            seat.BookingId = bookingId;

            await UpdateAsync(seat, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully reserved seat ID: {SeatId}", seatId);
            }

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            _logger.LogWarning("Concurrency conflict while reserving seat ID: {SeatId}", seatId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving seat ID: {SeatId}", seatId);
            throw;
        }
    }
}