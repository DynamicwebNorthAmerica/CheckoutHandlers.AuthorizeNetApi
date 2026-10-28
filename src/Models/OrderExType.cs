using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class OrderExType
{
    [JsonPropertyName("invoiceNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("purchaseOrderNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PurchaseOrderNumber { get; set; }
}
