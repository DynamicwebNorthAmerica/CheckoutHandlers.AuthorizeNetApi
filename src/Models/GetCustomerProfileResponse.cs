using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetCustomerProfileResponse
{
    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }

    [JsonPropertyName("profile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerProfileMaskedType? Profile { get; set; }
}
