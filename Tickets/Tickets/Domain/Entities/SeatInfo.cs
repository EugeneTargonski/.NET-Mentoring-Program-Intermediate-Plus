using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities
{
    public class SeatInfo
    {
        [JsonPropertyName("seatId")]
        public string SeatId { get; set; } = string.Empty;

        [JsonPropertyName("seatNumber")]
        public string SeatNumber { get; set; } = string.Empty;

        [JsonPropertyName("row")]
        public string? Row { get; set; }

        [JsonPropertyName("section")]
        public string? Section { get; set; }
    }
}
