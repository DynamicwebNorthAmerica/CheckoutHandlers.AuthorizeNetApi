using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerProfileExType
{
    [JsonPropertyName("merchantCustomerId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MerchantCustomerId { get; set; }

    [JsonPropertyName("customerProfileId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerProfileId { get; set; }
}
