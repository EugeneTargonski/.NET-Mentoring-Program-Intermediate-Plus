using System.Text.Json.Serialization;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entities
{
    public class OfferInfo
    {
        [JsonPropertyName("offerId")]
        public string OfferId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("priceCategory")]
        public PriceCategory PriceCategory { get; set; }
    }
}
