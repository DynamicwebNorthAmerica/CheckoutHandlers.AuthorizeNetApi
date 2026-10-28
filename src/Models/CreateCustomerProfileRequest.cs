using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateCustomerProfileRequest
{
    [JsonPropertyName("merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [JsonPropertyName("refId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefId { get; set; }

    [JsonPropertyName("profile")]
    public CustomerProfileType Profile { get; set; } = new();
}
