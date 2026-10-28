using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PaymentType
{
    [JsonPropertyName("creditCard"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CreditCardType? CreditCard { get; set; }

    [JsonPropertyName("trackData"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TrackData { get; set; }

    [JsonPropertyName("encryptedTrackData"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EncryptedTrackData { get; set; }

    [JsonPropertyName("payPal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PayPalType? PayPal { get; set; }

    [JsonPropertyName("emv"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentEmvType? Emv { get; set; }

    [JsonPropertyName("dataSource"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? DataSource { get; set; }

    [JsonPropertyName("paymentMaskedType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentMaskedType? PaymentMaskedType { get; set; }

    [JsonPropertyName("tokenMaskedType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TokenMaskedType? TokenMaskedType { get; set; }

    [JsonPropertyName("tokenRequestorId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TokenRequestorId { get; set; }
}
