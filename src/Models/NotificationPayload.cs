using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

/// <summary>
/// The "payload" object of a payment webhook notification.
/// See: https://developer.authorize.net/api/reference/features/webhooks.html
/// </summary>
internal sealed class NotificationPayload
{
    /// <summary>
    /// Transaction id (transId) the event is about.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("responseCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ResponseCode { get; set; }

    [JsonPropertyName("authCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AuthCode { get; set; }

    [JsonPropertyName("avsResponse"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AvsResponse { get; set; }

    private double _amount;

    [JsonPropertyName("authAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double Amount
    {
        get => _amount;
        set => _amount = AmountHelper.AdjustAmount(value);
    }

    /// <summary>
    /// The refId sent with the API request that created the transaction.
    /// </summary>
    [JsonPropertyName("merchantReferenceId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MerchantReferenceId { get; set; }

    /// <summary>
    /// order.invoiceNumber of the transaction, when Authorize.Net includes it.
    /// </summary>
    [JsonPropertyName("invoiceNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("entityName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    [JsonPropertyName("eventType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EventType { get; set; }
}
