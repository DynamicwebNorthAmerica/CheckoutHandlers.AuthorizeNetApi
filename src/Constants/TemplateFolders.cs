namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class TemplateFolders
{
    internal const string ErrorTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Error";
    internal const string CancelTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Cancel";
    internal const string PendingTemplateFolder = "eCom7/CheckoutHandler/AuthorizeNet/Pending";

    /// <summary>
    /// Installed by <see cref="Updates.AuthorizeNetUpdateProvider"/>. The pending page has no fallback, so a
    /// default is selected out of the box.
    /// </summary>
    internal const string DefaultPendingTemplate = "checkouthandler_pending.html";
}
