using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class OrderType
{
    [JsonPropertyName("invoiceNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? InvoiceNumber { get; set; }
}
