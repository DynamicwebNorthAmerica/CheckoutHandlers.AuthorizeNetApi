using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CardArt
{
    [JsonPropertyName("cardBrand"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardBrand { get; set; }

    [JsonPropertyName("cardImageHeight"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardImageHeight { get; set; }

    [JsonPropertyName("cardImageUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardImageUrl { get; set; }

    [JsonPropertyName("cardImageWidth"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardImageWidth { get; set; }

    [JsonPropertyName("cardType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardType { get; set; }
}


