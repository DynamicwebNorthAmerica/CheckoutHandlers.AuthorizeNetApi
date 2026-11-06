using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Self link in webhook response
/// </summary>
internal sealed class WebhookSelfLink
{
    /// <summary>
    /// HREF to the webhook resource
    /// </summary>
    [JsonPropertyName("href"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Href { get; set; }
}
