using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class DriversLicenseMaskedType
{
    [JsonPropertyName("number"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Number { get; set; }

    [JsonPropertyName("state"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? State { get; set; }

    [JsonPropertyName("dateOfBirth"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? DateOfBirth { get; set; }
}
