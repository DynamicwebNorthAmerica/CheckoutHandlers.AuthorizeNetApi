using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class SplitTenderPayment
{
    [JsonPropertyName("transId")]
    public string TransId { get; set; } = "";

    [JsonPropertyName("responseCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ResponseCode { get; set; }

    [JsonPropertyName("responseToCustomer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ResponseToCustomer { get; set; }

    [JsonPropertyName("authCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AuthCode { get; set; }

    [JsonPropertyName("accountNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("accountType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountType { get; set; }

    [JsonPropertyName("requestedAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RequestedAmount { get; set; }

    [JsonPropertyName("approvedAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ApprovedAmount { get; set; }

    [JsonPropertyName("balanceOnCard"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BalanceOnCard { get; set; }
}
