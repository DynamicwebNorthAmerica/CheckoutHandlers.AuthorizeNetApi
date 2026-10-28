using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class HostedPaymentSettings
{
    [JsonPropertyName("setting")]
    public IEnumerable<Setting> Setting { get; set; } = [];
}