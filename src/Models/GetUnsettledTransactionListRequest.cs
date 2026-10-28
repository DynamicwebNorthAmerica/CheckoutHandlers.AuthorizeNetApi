using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Request for getUnsettledTransactionListRequest: lists transactions that have not been settled yet.
/// See: https://developer.authorize.net/api/reference/index.html#transaction-reporting-get-unsettled-transaction-list
/// </summary>
internal sealed class GetUnsettledTransactionListRequest
{
    [JsonPropertyName("merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [JsonPropertyName("refId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefId { get; set; }

    /// <summary>
    /// Optional filter. "pendingApproval" returns only transactions held for review.
    /// </summary>
    [JsonPropertyName("status"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Status { get; set; }

    [JsonPropertyName("sorting"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TransactionListSorting? Sorting { get; set; }

    [JsonPropertyName("paging"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TransactionListPaging? Paging { get; set; }
}
