using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PrePaidCard
{
    [JsonPropertyName("requestedAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RequestedAmount { get; set; }

    [JsonPropertyName("approvedAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ApprovedAmount { get; set; }

    [JsonPropertyName("balanceOnCard"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BalanceOnCard { get; set; }
}
