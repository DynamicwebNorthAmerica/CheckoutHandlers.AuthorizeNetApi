using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Reads the parts of an Authorize.Net webhook notification the handler needs.
/// </summary>
internal static class WebhookPayloadReader
{
    /// <summary>
    /// Parses the notification body. Returns null when the body is not a notification with a payload.
    /// </summary>
    public static NotificationItem? Parse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        return AuthorizeNetJson.TryDeserialize(body, out NotificationItem? item) && item.Payload is not null
            ? item
            : null;
    }

    /// <summary>
    /// Resolves the Dynamicweb order id for a notification.
    /// The refId sent with every request comes back as merchantReferenceId, which is the documented
    /// correlation field. invoiceNumber (order.invoiceNumber on the transaction) is the fallback.
    /// </summary>
    public static bool TryGetOrderId(NotificationItem? item, [NotNullWhen(true)] out string? orderId)
    {
        orderId = null;
        NotificationPayload? payload = item?.Payload;
        if (payload is null)
            return false;

        if (!string.IsNullOrWhiteSpace(payload.MerchantReferenceId))
            orderId = payload.MerchantReferenceId.Trim();
        else if (!string.IsNullOrWhiteSpace(payload.InvoiceNumber))
            orderId = payload.InvoiceNumber.Trim();

        return orderId is not null;
    }
}
