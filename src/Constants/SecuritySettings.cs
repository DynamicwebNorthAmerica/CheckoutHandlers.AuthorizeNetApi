namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class SecuritySettings
{
    /// <summary>
    /// Mask for hiding card number in logs (show only last 4 digits)
    /// </summary>
    internal const string CardNumberMask = "XXXX-XXXX-XXXX-";

    /// <summary>
    /// Expiration date value Authorize.Net accepts for refunds against a stored transaction.
    /// </summary>
    internal const string CreditCardExpirationMask = "XXXX";

    /// <summary>
    /// Replacement for secrets in logs
    /// </summary>
    internal const string SecretMask = "***HIDDEN***";

    /// <summary>
    /// Prefix stored in Order.GatewayPaymentStatus while a hosted-form payment waits for the
    /// transaction data needed to save the card.
    /// </summary>
    internal const string SavedCardNamePlaceholder = "NeedToSaveCardWithName:";

    /// <summary>
    /// Maximum length Authorize.Net allows for refId (merchant reference id).
    /// </summary>
    internal const int RefIdMaxLength = 20;
}
