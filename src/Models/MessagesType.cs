using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class MessagesType
{
    [JsonPropertyName("resultCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ResultCode { get; set; }

    [JsonPropertyName("message"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Message[]? Message { get; set; }
}
