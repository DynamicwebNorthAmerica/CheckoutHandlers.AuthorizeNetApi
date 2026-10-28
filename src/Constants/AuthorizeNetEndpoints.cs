namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;

internal static class AuthorizeNetEndpoints
{
    private const string ProductionApi = "https://api.authorize.net/xml/v1/request.api";
    private const string SandboxApi = "https://apitest.authorize.net/xml/v1/request.api";

    private const string ProductionHostedForm = "https://accept.authorize.net/payment/payment";
    private const string SandboxHostedForm = "https://test.authorize.net/payment/payment";

    private const string ProductionWebhookBase = "https://api.authorize.net";
    private const string SandboxWebhookBase = "https://apitest.authorize.net";

    public static string GetApiEndpoint(bool isTestMode) => isTestMode ? SandboxApi : ProductionApi;

    public static string GetHostedFormUrl(bool isTestMode) => isTestMode ? SandboxHostedForm : ProductionHostedForm;

    public static string GetWebhookBaseUrl(bool isTestMode) => isTestMode ? SandboxWebhookBase : ProductionWebhookBase;
}
