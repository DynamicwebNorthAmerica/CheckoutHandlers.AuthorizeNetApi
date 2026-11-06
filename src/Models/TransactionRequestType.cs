using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TransactionRequestType
{
    private double _amount;

    [JsonPropertyName("transactionType")]
    public TransactionTypeEnum TransactionType { get; set; }

    [JsonPropertyName("amount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double Amount
    {
        get => _amount;
        set => _amount = AmountHelper.AdjustAmount(value);
    }

    [JsonPropertyName("currencyCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("payment"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentType? Payment { get; set; }

    [JsonPropertyName("profile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerProfilePaymentType? Profile { get; set; }

    [JsonPropertyName("refTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefTransId { get; set; }

    [JsonPropertyName("order"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public OrderType? Order { get; set; }

    [JsonPropertyName("lineItems"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public LineItems? LineItems { get; set; }

    [JsonPropertyName("customer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerDataType? Customer { get; set; }

    [JsonPropertyName("billTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerAddressType? BillTo { get; set; }

    [JsonPropertyName("shipTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public NameAndAddressType? ShipTo { get; set; }

    [JsonPropertyName("customerIP"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerIp { get; set; }

    [JsonPropertyName("processingOptions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ProcessingOptionsType? ProcessingOptions { get; set; }

    [JsonPropertyName("subsequentAuthInformation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SubsequentAuthInformationType? SubsequentAuthInformation { get; set; }
}
