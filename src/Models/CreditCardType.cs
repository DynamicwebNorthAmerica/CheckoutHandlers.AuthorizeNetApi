using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreditCardType
{
    [JsonPropertyName("cardNumber")]
    public string CardNumber { get; set; } = "";

    [JsonPropertyName("expirationDate")]
    public string ExpirationDate { get; set; } = "";
}