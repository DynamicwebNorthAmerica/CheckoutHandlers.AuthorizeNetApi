using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class MerchantAuthenticationType
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("transactionKey")]
    public string TransactionKey { get; set; } = "";
}