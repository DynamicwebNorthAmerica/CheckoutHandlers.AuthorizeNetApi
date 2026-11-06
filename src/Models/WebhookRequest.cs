using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// Represents a webhook request for Authorize.Net Webhooks API
/// </summary>
internal sealed class WebhookRequest
{
    /// <summary>
    /// Name of the webhook (optional)
    /// </summary>
    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    /// <summary>
    /// URL to receive webhook notifications
    /// </summary>
    [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Url { get; set; }

    /// <summary>
    /// Array of event types to subscribe to
    /// </summary>
    [JsonPropertyName("eventTypes"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string[]? EventTypes { get; set; }

    /// <summary>
    /// Status of the webhook (active/inactive) - optional
    /// </summary>
    [JsonPropertyName("status"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Status { get; set; }
}
