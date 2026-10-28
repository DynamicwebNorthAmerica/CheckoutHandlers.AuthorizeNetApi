using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Standard error response from Authorize.Net API
/// </summary>
internal sealed class ErrorResponse
{
    [JsonPropertyName("refId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefId { get; set; }

    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MessagesType? Messages { get; set; }
}
