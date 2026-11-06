using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerProfileIdType
{
    [JsonPropertyName("customerProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerProfileId { get; set; }

    [JsonPropertyName("customerPaymentProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerPaymentProfileId { get; set; }

    [JsonPropertyName("customerAddressId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerAddressId { get; set; }
}
