using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class HostedPaymentPaymentOptions
{
    [JsonPropertyName("showBankAccount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ShowBankAccount { get; set; }
}