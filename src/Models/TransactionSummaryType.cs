using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// One row of a transaction list (transactionSummaryType).
/// </summary>
internal sealed class TransactionSummaryType
{
    [JsonPropertyName("transId")]
    public string TransId { get; set; } = "";

    [JsonPropertyName("submitTimeUTC"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SubmitTimeUTC { get; set; }

    [JsonPropertyName("submitTimeLocal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SubmitTimeLocal { get; set; }

    [JsonPropertyName("transactionStatus"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TransactionStatusEnum? TransactionStatus { get; set; }

    [JsonPropertyName("invoiceNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("firstName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? LastName { get; set; }

    [JsonPropertyName("accountType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountType { get; set; }

    [JsonPropertyName("accountNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("settleAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double SettleAmount { get; set; }

    [JsonPropertyName("marketType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MarketType { get; set; }

    [JsonPropertyName("product"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Product { get; set; }

    [JsonPropertyName("hasReturnedItems"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HasReturnedItems { get; set; }

    [JsonPropertyName("profile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerProfileIdType? Profile { get; set; }
}
