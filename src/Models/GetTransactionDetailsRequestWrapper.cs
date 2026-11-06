using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetTransactionDetailsRequestWrapper
{
    [JsonPropertyName("getTransactionDetailsRequest")]
    public GetTransactionDetailsRequest GetTransactionDetailsRequest { get; set; } = new();
}