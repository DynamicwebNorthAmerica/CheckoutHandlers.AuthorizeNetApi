using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PayPalType
{
    [JsonPropertyName("successUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SuccessUrl { get; set; }

    [JsonPropertyName("cancelUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CancelUrl { get; set; }

    [JsonPropertyName("paypalLc"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PaypalLc { get; set; }

    [JsonPropertyName("paypalHdrImg"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PaypalHdrImg { get; set; }

    [JsonPropertyName("paypalPayflowcolor"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PaypalPayflowcolor { get; set; }

    [JsonPropertyName("payerID"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PayerID { get; set; }
}
