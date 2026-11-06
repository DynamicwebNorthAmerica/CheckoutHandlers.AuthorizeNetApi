using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Decides whether the webhook subscription for a callback URL must be (re)created.
/// </summary>
internal static class WebhookRegistrationPolicy
{
    public const string WebhookName = "DynamicwebAuthorizeNetWebhook";

    /// <summary>
    /// Event types the handler processes. All of them must be subscribed.
    /// </summary>
    public static readonly string[] RequiredEventTypes =
    [
        "net.authorize.payment.authorization.created",
        "net.authorize.payment.authcapture.created",
        "net.authorize.payment.capture.created",
        "net.authorize.payment.priorAuthCapture.created",
        "net.authorize.payment.refund.created",
        "net.authorize.payment.void.created",
        "net.authorize.payment.fraud.held",
        "net.authorize.payment.fraud.approved",
        "net.authorize.payment.fraud.declined"
    ];

    public static WebhookRegistrationResult Decide(IReadOnlyList<WebhookResponse> existingWebhooksForUrl, bool forceRegistration)
    {
        if (forceRegistration)
            return new WebhookRegistrationResult(true, "Force re-registration is enabled");

        if (existingWebhooksForUrl.Count == 0)
            return new WebhookRegistrationResult(true, "No existing webhooks found for this URL");

        if (existingWebhooksForUrl.Count > 1)
            return new WebhookRegistrationResult(true, "Multiple webhooks found - consolidating into one");

        WebhookResponse webhook = existingWebhooksForUrl[0];
        var registeredEvents = new HashSet<string>(webhook.EventTypes ?? [], StringComparer.OrdinalIgnoreCase);
        List<string> missingEvents = RequiredEventTypes.Where(e => !registeredEvents.Contains(e)).ToList();

        if (missingEvents.Count > 0)
            return new WebhookRegistrationResult(true, $"Missing required events: {string.Join(", ", missingEvents)}");

        if (!string.Equals(webhook.Status, "active", StringComparison.OrdinalIgnoreCase))
            return new WebhookRegistrationResult(true, "Existing webhook is not active");

        return new WebhookRegistrationResult(false, "All required webhooks are properly configured");
    }

    public static bool MatchesUrl(WebhookResponse webhook, string webhookUrl)
        => string.Equals(webhook.Url?.TrimEnd('/'), webhookUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
}
