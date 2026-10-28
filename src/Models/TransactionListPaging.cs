using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TransactionListPaging
{
    /// <summary>
    /// Number of transactions per page (1-1000).
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 100;

    /// <summary>
    /// 1-based page number.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; } = 1;
}
