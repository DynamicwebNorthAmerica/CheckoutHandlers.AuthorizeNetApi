using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TransactionListSorting
{
    /// <summary>
    /// Field to sort by: "id" or "submitTimeUTC".
    /// </summary>
    [JsonPropertyName("orderBy")]
    public string OrderBy { get; set; } = "submitTimeUTC";

    [JsonPropertyName("orderDescending")]
    public bool OrderDescending { get; set; } = true;
}
