using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetHostedPaymentPageResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = "";

    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }
}
