using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TransactionDetailsType
{
    [JsonPropertyName("transId")]
    public string TransId { get; set; } = "";

    [JsonPropertyName("refTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefTransId { get; set; }

    [JsonPropertyName("splitTenderId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SplitTenderId { get; set; }

    [JsonPropertyName("submitTimeUTC"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SubmitTimeUTC { get; set; }

    [JsonPropertyName("submitTimeLocal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SubmitTimeLocal { get; set; }

    [JsonPropertyName("transactionType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TransactionTypeEnum? TransactionType { get; set; }

    [JsonPropertyName("transactionStatus"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TransactionStatusEnum? TransactionStatus { get; set; }

    [JsonPropertyName("responseCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ResponseCode { get; set; }

    [JsonPropertyName("responseReasonCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ResponseReasonCode { get; set; }

    [JsonPropertyName("subscription"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SubscriptionPaymentType? Subscription { get; set; }

    [JsonPropertyName("responseReasonDescription"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ResponseReasonDescription { get; set; }

    [JsonPropertyName("authCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AuthCode { get; set; }

    [JsonPropertyName("AVSResponse"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AvsResponse { get; set; }

    [JsonPropertyName("cardCodeResponse"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CardCodeResponse { get; set; }

    [JsonPropertyName("CAVVResponse"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CavvResponse { get; set; }

    [JsonPropertyName("FDSFilterAction"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public FdsFilterActionEnum? FdsFilterAction { get; set; }

    [JsonPropertyName("FDSFilters"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<FDSFilter>? FdsFilters { get; set; }

    [JsonPropertyName("batch"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public BatchDetailsType? Batch { get; set; }

    [JsonPropertyName("order"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public OrderExType? Order { get; set; }

    [JsonPropertyName("requestedAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double RequestedAmount { get; set; }

    [JsonPropertyName("authAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double AuthAmount { get; set; }

    [JsonPropertyName("settleAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double SettleAmount { get; set; }

    [JsonPropertyName("tax"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ExtendedAmountType? Tax { get; set; }

    [JsonPropertyName("shipping"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ExtendedAmountType? Shipping { get; set; }

    [JsonPropertyName("duty"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ExtendedAmountType? Duty { get; set; }

    [JsonPropertyName("lineItems"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<LineItem>? LineItems { get; set; }

    [JsonPropertyName("prepaidBalanceRemaining"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double PrepaidBalanceRemaining { get; set; }

    [JsonPropertyName("taxExempt"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool TaxExempt { get; set; }

    [JsonPropertyName("payment"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentMaskedType? Payment { get; set; }

    [JsonPropertyName("customer"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerDataType? Customer { get; set; }

    [JsonPropertyName("billTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerAddressType? BillTo { get; set; }

    [JsonPropertyName("shipTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public NameAndAddressType? ShipTo { get; set; }

    [JsonPropertyName("recurringBilling"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool RecurringBilling { get; set; }

    [JsonPropertyName("customerIP"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerIp { get; set; }

    [JsonPropertyName("product"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Product { get; set; }

    [JsonPropertyName("entryMode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EntryMode { get; set; }

    [JsonPropertyName("marketType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MarketType { get; set; }

    [JsonPropertyName("mobileDeviceId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MobileDeviceId { get; set; }

    [JsonPropertyName("customerSignature"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerSignature { get; set; }

    [JsonPropertyName("returnedItems"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<ReturnedItem>? ReturnedItems { get; set; }

    [JsonPropertyName("solution"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SolutionType? Solution { get; set; }

    [JsonPropertyName("profile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerProfileIdType? Profile { get; set; }

    [JsonPropertyName("surcharge"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ExtendedAmountType? Surcharge { get; set; }

    [JsonPropertyName("employeeId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EmployeeId { get; set; }

    [JsonPropertyName("tip"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ExtendedAmountType? Tip { get; set; }

    [JsonPropertyName("otherTax"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public OtherTaxType? OtherTax { get; set; }

    [JsonPropertyName("shipFrom"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public NameAndAddressType? ShipFrom { get; set; }

    [JsonPropertyName("networkTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? NetworkTransId { get; set; }

    [JsonPropertyName("originalNetworkTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? OriginalNetworkTransId { get; set; }

    [JsonPropertyName("originalAuthAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double OriginalAuthAmount { get; set; }

    [JsonPropertyName("authorizationIndicator"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AuthorizationIndicator { get; set; }
}
