using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class ReturnedItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("dateUTC"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? DateUTC { get; set; }

    [JsonPropertyName("dateLocal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? DateLocal { get; set; }

    [JsonPropertyName("code"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Code { get; set; }

    [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Description { get; set; }
}
