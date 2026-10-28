using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class OtherTaxType
{
    [JsonPropertyName("nationalTaxAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double NationalTaxAmount { get; set; }

    [JsonPropertyName("localTaxAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double LocalTaxAmount { get; set; }

    [JsonPropertyName("alternateTaxAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double AlternateTaxAmount { get; set; }

    [JsonPropertyName("alternateTaxId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AlternateTaxId { get; set; }

    [JsonPropertyName("vatTaxRate"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double VatTaxRate { get; set; }

    [JsonPropertyName("vatTaxAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double VatTaxAmount { get; set; }
}
