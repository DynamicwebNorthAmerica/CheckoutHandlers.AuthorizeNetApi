using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TokenMaskedType
{
    [JsonPropertyName("tokenSource")]
    public string TokenSource { get; set; } = "";

    [JsonPropertyName("tokenNumber")]
    public string TokenNumber { get; set; } = "";

    [JsonPropertyName("expirationDate")]
    public string ExpirationDate { get; set; } = "";

    [JsonPropertyName("tokenRequestorId")]
    public string TokenRequestorId { get; set; } = "";
}