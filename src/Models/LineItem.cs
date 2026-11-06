using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class LineItem
{
    [JsonPropertyName("itemId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ItemId { get; set; }

    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Description { get; set; }

    [JsonPropertyName("quantity"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double Quantity { get; set; }

    [JsonPropertyName("unitPrice"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double UnitPrice { get; set; }
}
