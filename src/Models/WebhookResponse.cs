using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Represents a webhook response from Authorize.Net Webhooks API
/// </summary>
internal sealed class WebhookResponse
{
    /// <summary>
    /// Unique identifier for the webhook
    /// </summary>
    [JsonPropertyName("webhookId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? WebhookId { get; set; }

    /// <summary>
    /// Name of the webhook
    /// </summary>
    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    /// <summary>
    /// Array of event types the webhook is subscribed to
    /// </summary>
    [JsonPropertyName("eventTypes"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string[]? EventTypes { get; set; }

    /// <summary>
    /// Current status of the webhook
    /// </summary>
    [JsonPropertyName("status"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Status { get; set; } = "active";

    /// <summary>
    /// URL where webhook notifications are sent
    /// </summary>
    [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Url { get; set; }

    /// <summary>
    /// Links section from response
    /// </summary>
    [JsonPropertyName("_links"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public WebhookLinks? Links { get; set; }
}
