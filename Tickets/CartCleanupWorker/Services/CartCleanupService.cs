using CartCleanupWorker.Configuration;
using CartCleanupWorker.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CartCleanupWorker.Services;

/// <summary>
/// Service responsible for cleaning up expired cart items by releasing held seats
/// Works directly with Cosmos DB to release seats that have been held longer than expiration time
/// </summary>
public class CartCleanupService : ICartCleanupService
{
    private readonly ILogger<CartCleanupService> _logger;
    private readonly CartCleanupSettings _settings;
    private readonly Container _container;

    public CartCleanupService(
        ILogger<CartCleanupService> logger,
        IOptions<CartCleanupSettings> settings,
        CosmosClient cosmosClient,
        CosmosDbSettings cosmosDbSettings)
    {
        _logger = logger;
        _settings = settings.Value;
        _container = cosmosClient.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.ContainerName);
    }

    public async Task CleanupExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        var expirationTime = DateTime.UtcNow;
        var totalReleased = 0;

        _logger.LogInformation("Starting cart cleanup. Releasing seats held before {expirationTime}", expirationTime);

        try
        {
            // Query for seats that are OnHold and have expired
            var query = new QueryDefinition(
                @"SELECT * FROM c 
                  WHERE c.entityType = 'Seat' 
                  AND c.status = 'OnHold' 
                  AND c.holdExpiresAt < @expirationTime")
                .WithParameter("@expirationTime", expirationTime);

            var iterator = _container.GetItemQueryIterator<Seat>(query);

            var expiredSeats = new List<Seat>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                expiredSeats.AddRange(response);
            }

            _logger.LogInformation("Found {count} expired seat holds to release", expiredSeats.Count);

            // Release each expired seat
            foreach (var seat in expiredSeats)
            {
                try
                {
                    // Update seat to Available status
                    seat.Status = "Available";
                    seat.HoldExpiresAt = null;
                    seat.HeldByCustomerId = null;

                    var requestOptions = new ItemRequestOptions
                    {
                        IfMatchEtag = seat.ETag
                    };

                    await _container.ReplaceItemAsync(
                        seat,
                        seat.Id,
                        new PartitionKey(seat.EventId),
                        requestOptions,
                        cancellationToken);

                    totalReleased++;

                    _logger.LogDebug(
                        "Released seat {seatId} in event {eventId} (held by {customerId})",
                        seat.Id, seat.EventId, seat.HeldByCustomerId);
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogWarning(
                        "Concurrency conflict releasing seat {seatId} - seat was already modified",
                        seat.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing seat {seatId}", seat.Id);
                }
            }

            if (totalReleased > 0)
            {
                _logger.LogInformation("Cart cleanup completed. Released {count} expired seat holds", totalReleased);
            }
            else
            {
                _logger.LogDebug("Cart cleanup completed. No expired seat holds found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cart cleanup");
            throw;
        }
    }
}
