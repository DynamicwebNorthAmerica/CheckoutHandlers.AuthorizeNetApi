using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Links section in webhook response
/// </summary>
internal sealed class WebhookLinks
{
    /// <summary>
    /// Self link
    /// </summary>
    [JsonPropertyName("self"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public WebhookSelfLink? Self { get; set; }
}