using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities
{
    public class EventInfo
    {
        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("venueName")]
        public string? VenueName { get; set; }
    }
}
