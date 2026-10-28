using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetCustomerProfileRequestWrapper
{
    [JsonPropertyName("getCustomerProfileRequest")]
    public GetCustomerProfileRequest GetCustomerProfileRequest { get; set; } = new();
}