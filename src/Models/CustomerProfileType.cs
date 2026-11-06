using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerProfileType
{
    [JsonPropertyName("merchantCustomerId")]
    public string MerchantCustomerId { get; set; } = "";
}