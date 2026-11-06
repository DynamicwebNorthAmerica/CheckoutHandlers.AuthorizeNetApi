using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class EmvResponse
{
    [JsonPropertyName("tlvData"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TlvData { get; set; }

    [JsonPropertyName("tags"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EmvTag? Tags { get; set; }
}
