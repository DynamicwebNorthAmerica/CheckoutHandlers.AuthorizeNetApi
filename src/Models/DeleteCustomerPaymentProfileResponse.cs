using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class DeleteCustomerPaymentProfileResponse
{
    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }
}
