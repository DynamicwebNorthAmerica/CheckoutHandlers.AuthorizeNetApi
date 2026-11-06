using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateCustomerProfileRequestWrapper
{
    [JsonPropertyName("createCustomerProfileRequest")]
    public CreateCustomerProfileRequest CreateCustomerProfileRequest { get; set; } = new();
}