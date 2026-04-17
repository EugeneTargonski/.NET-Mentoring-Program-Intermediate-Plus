using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities
{
    public class VenueInfo
    {
        [JsonPropertyName("venueId")]
        public string VenueId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }
    }
}
