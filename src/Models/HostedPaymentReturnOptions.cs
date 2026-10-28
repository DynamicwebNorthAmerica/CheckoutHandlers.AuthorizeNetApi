using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class HostedPaymentReturnOptions
{
    /// <summary>
    /// When true the hosted page shows its own receipt with a "Continue" button that leads to Url.
    /// When false the customer is redirected to Url right after the payment.
    /// </summary>
    [JsonPropertyName("showReceipt")]
    public bool ShowReceipt { get; set; }

    [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Url { get; set; }

    [JsonPropertyName("urlText"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? UrlText { get; set; }

    [JsonPropertyName("cancelUrl"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CancelUrl { get; set; }

    [JsonPropertyName("cancelUrlText"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CancelUrlText { get; set; }
}
