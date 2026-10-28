using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class SubscriptionPaymentType
{
    [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; set; }

    [JsonPropertyName("payNum"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PayNum { get; set; }
}
