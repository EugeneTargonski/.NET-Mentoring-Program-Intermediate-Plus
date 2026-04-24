using Tickets.Data.Abstractions;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services;

public class EventService(IUnitOfWork unitOfWork) : IEventService
{
    public async Task<IEnumerable<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = await unitOfWork.Events.GetAllAsync(cancellationToken);

        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description,
            e.EventDate,
            e.EventEndDate,
            e.VenueId,
            e.Category,
            e.IsActive
        ));
    }

    public async Task<IEnumerable<EventSeatDto>> GetEventSeatsAsync(
        string eventId, 
        string sectionId, 
        CancellationToken cancellationToken = default)
    {
        var seats = await unitOfWork.Seats.QueryAsync(
            s => s.EventId == eventId && s.Section == sectionId,
            eventId,
            cancellationToken);

        var result = new List<EventSeatDto>();

        foreach (var seat in seats)
        {
            PriceOptionDto? priceOption = null;

            if (!string.IsNullOrEmpty(seat.CurrentOfferId))
            {
                var offer = await unitOfWork.Offers.GetByIdAsync(
                    seat.CurrentOfferId, 
                    seat.CurrentOfferId, 
                    cancellationToken);

                if (offer != null)
                {
                    priceOption = new PriceOptionDto(
                        offer.Id,
                        offer.Name,
                        offer.Price
                    );
                }
            }

            result.Add(new EventSeatDto(
                seat.Id,
                seat.Section ?? string.Empty,
                seat.Row,
                seat.SeatNumber,
                seat.Status.ToString(),
                priceOption
            ));
        }

        return result;
    }
}
