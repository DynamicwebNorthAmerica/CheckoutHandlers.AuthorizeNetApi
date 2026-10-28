using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class FDSFilter
{
    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    [JsonPropertyName("action"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public FdsFilterActionEnum? Action { get; set; }
}
