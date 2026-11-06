using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetUnsettledTransactionListResponse
{
    [JsonPropertyName("transactions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<TransactionSummaryType>? Transactions { get; set; }

    [JsonPropertyName("totalNumInResultSet"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TotalNumInResultSet { get; set; }

    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }
}
