using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateCustomerProfileFromTransactionRequest
{
    [JsonPropertyName("merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [JsonPropertyName("refId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefId { get; set; }

    [JsonPropertyName("transId")]
    public string TransId { get; set; } = "";

    [JsonPropertyName("customerProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerProfileId { get; set; }
}
