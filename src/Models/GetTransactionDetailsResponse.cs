using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetTransactionDetailsResponse
{
    [JsonPropertyName("transaction")]
    public TransactionDetailsType Transaction { get; set; } = new();

    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }
}
