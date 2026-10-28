using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PaymentProfile
{
    [JsonPropertyName("paymentProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PaymentProfileId { get; set; }
}
