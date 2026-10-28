using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateCustomerProfileResponse
{
    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }

    [JsonPropertyName("customerProfileId")]
    public string CustomerProfileId { get; set; } = "";

    [JsonPropertyName("customerPaymentProfileIdList"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<string>? CustomerPaymentProfileIdList { get; set; }
}
