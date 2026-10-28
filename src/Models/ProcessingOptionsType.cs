using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class ProcessingOptionsType
{
    [JsonPropertyName("isSubsequentAuth"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsSubsequentAuth { get; set; }

    [JsonPropertyName("isStoredCredentials"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsStoredCredentials { get; set; }
}
