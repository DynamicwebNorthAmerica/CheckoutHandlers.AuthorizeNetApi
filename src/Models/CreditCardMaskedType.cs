using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreditCardMaskedType
{
    [JsonPropertyName("cardNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardNumber { get; set; }

    [JsonPropertyName("expirationDate"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ExpirationDate { get; set; }

    [JsonPropertyName("cardType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CardTypeEnum? CardType { get; set; }

    [JsonPropertyName("cardArt"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CardArt? CardArt { get; set; }

    [JsonPropertyName("issuerNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? IssuerNumber { get; set; }

    [JsonPropertyName("isPaymentToken"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsPaymentToken { get; set; }
}
