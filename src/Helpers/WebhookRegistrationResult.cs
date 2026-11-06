namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Result of webhook registration analysis
/// </summary>
internal sealed class WebhookRegistrationResult
{
    /// <summary>
    /// Whether webhook registration is required
    /// </summary>
    public bool RequiresRegistration { get; }

    /// <summary>
    /// Reason for the registration decision
    /// </summary>
    public string Reason { get; }

    public WebhookRegistrationResult(bool requiresRegistration, string reason)
    {
        RequiresRegistration = requiresRegistration;
        Reason = reason;
    }
}