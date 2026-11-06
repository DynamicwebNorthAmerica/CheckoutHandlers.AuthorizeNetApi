using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerPaymentProfileMaskedType
{
    [JsonPropertyName("customerType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerTypeEnum? CustomerType { get; set; }

    [JsonPropertyName("billTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerAddressType? BillTo { get; set; }

    [JsonPropertyName("customerProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerProfileId { get; set; }

    [JsonPropertyName("customerPaymentProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerPaymentProfileId { get; set; }

    [JsonPropertyName("defaultPaymentProfile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool DefaultPaymentProfile { get; set; }

    [JsonPropertyName("payment"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentMaskedType? Payment { get; set; }

    [JsonPropertyName("driversLicense"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DriversLicenseMaskedType? DriversLicense { get; set; }

    [JsonPropertyName("taxId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TaxId { get; set; }

    [JsonPropertyName("subscriptionIds"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<string>? SubscriptionIds { get; set; }

    [JsonPropertyName("originalNetworkTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? OriginalNetworkTransId { get; set; }

    [JsonPropertyName("originalAuthAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double OriginalAuthAmount { get; set; }
}
