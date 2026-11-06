using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class LineItems
{
    [JsonPropertyName("lineItem"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<LineItem>? LineItem { get; set; }
}