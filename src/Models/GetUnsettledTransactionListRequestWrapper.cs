using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetUnsettledTransactionListRequestWrapper
{
    [JsonPropertyName("getUnsettledTransactionListRequest")]
    public GetUnsettledTransactionListRequest GetUnsettledTransactionListRequest { get; set; } = new();
}
