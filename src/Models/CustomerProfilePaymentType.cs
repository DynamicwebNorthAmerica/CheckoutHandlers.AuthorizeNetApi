using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerProfilePaymentType
{
    [JsonPropertyName("customerProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerProfileId { get; set; }

    [JsonPropertyName("paymentProfile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentProfile? PaymentProfile { get; set; }
}
