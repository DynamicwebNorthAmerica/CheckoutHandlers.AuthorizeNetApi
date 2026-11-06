using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetHostedPaymentPageRequestWrapper
{
    [JsonPropertyName("getHostedPaymentPageRequest")]
    public GetHostedPaymentPageRequest GetHostedPaymentPageRequest { get; set; } = new();
}