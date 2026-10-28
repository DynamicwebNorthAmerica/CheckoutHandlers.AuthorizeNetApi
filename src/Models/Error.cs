using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class Error
{
    [JsonPropertyName("errorCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("errorText"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ErrorText { get; set; }
}
