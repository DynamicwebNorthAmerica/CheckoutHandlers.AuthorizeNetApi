using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class SecureAcceptance
{
    [JsonPropertyName("secureAcceptanceUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SecureAcceptanceUrl { get; set; }

    [JsonPropertyName("payerID"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PayerID { get; set; }

    [JsonPropertyName("payerEmail"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PayerEmail { get; set; }
}
