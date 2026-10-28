using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PaymentEmvType
{
    [JsonPropertyName("emvData"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EmvData { get; set; }

    [JsonPropertyName("emvDescriptor"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EmvDescriptor { get; set; }

    [JsonPropertyName("emvVersion"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EmvVersion { get; set; }
}
