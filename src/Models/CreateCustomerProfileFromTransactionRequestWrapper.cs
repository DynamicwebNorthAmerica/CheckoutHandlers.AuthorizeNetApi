using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateCustomerProfileFromTransactionRequestWrapper
{
    [JsonPropertyName("createCustomerProfileFromTransactionRequest")]
    public CreateCustomerProfileFromTransactionRequest CreateCustomerProfileFromTransactionRequest { get; set; } = new();
}


