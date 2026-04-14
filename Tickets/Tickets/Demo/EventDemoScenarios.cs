using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Demonstrate Event Service operations (EventDb)
/// </summary>
public class EventDemoScenarios(IUnitOfWork unitOfWork, ILogger<EventDemoScenarios> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<EventDemoScenarios> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllAsync()
    {
        await CreateVenuesAsync();
        await CreateOffersAsync();
        await CreateManifestsAsync();
        await CreateEventsAsync();
    }

    private async Task CreateVenuesAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("--- Demo: Create Venues (EventDb) ---");
        }

        var venues = new[]
        {
            new Venue { Name = "Grand Arena", Address = "123 Main St", City = "New York", Country = "USA", Capacity = 20000 },
            new Venue { Name = "City Theater", Address = "456 Oak Ave", City = "Los Angeles", Country = "USA", Capacity = 5000 }
        };

        await _unitOfWork.Venues.CreateBulkAsync(venues);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created {Count} venues in EventDb", venues.Length);
        }
    }

    private async Task CreateOffersAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("--- Demo: Create Offers (EventDb) ---");
        }

        var offers = new[]
        {
            new Offer { Name = "Adult Ticket", Price = 50.00m, PriceCategory = PriceCategory.Adult, IsActive = true },
            new Offer { Name = "Child Ticket", Price = 25.00m, PriceCategory = PriceCategory.Child, IsActive = true },
            new Offer { Name = "VIP Ticket", Price = 150.00m, PriceCategory = PriceCategory.VIP, IsActive = true }
        };

        await _unitOfWork.Offers.CreateBulkAsync(offers);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created {Count} offers in EventDb", offers.Length);
        }
    }

    private async Task CreateManifestsAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("--- Demo: Create Manifests (EventDb) ---");
        }

        var venues = await _unitOfWork.Venues.GetAllAsync();
        var venuesList = venues.ToList();

        if (venuesList.Count == 0)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No venues found, skipping manifest creation");
            }
            return;
        }

        var manifests = new[]
        {
            new Manifest { Name = "Standard Seating", VenueId = venuesList[0].Id },
            new Manifest { Name = "Theater Seating", VenueId = venuesList.Count > 1 ? venuesList[1].Id : venuesList[0].Id }
        };

        foreach (var manifest in manifests)
            await _unitOfWork.Manifests.CreateAsync(manifest);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created {Count} manifests in EventDb", manifests.Length);
        }
    }

    private async Task CreateEventsAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("--- Demo: Create Events (EventDb) ---");
        }

        var venues = await _unitOfWork.Venues.GetAllAsync();
        var manifests = await _unitOfWork.Manifests.GetAllAsync();
        var firstVenue = venues.FirstOrDefault();
        var firstManifest = manifests.FirstOrDefault();

        if (firstVenue == null || firstManifest == null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("No venues or manifests found, skipping event creation");
            }
            return;
        }

        var newEvent = new Event
        {
            Name = "Rock Concert 2026",
            EventDate = DateTime.UtcNow.AddMonths(3),
            VenueId = firstVenue.Id,
            ManifestId = firstManifest.Id,
            IsActive = true,
            Venue = new VenueInfo 
            { 
                VenueId = firstVenue.Id, 
                Name = firstVenue.Name, 
                City = firstVenue.City, 
                Capacity = firstVenue.Capacity 
            }
        };

        await _unitOfWork.Events.CreateAsync(newEvent);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created event in EventDb: {EventName}", newEvent.Name);
        }
    }
}