using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class GetHostedPaymentPageRequest
{
    [JsonPropertyName("merchantAuthentication")]
    public MerchantAuthenticationType MerchantAuthentication { get; set; } = new();

    [JsonPropertyName("refId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefId { get; set; }

    [JsonPropertyName("transactionRequest")]
    public TransactionRequestType TransactionRequest { get; set; } = new();

    [JsonPropertyName("hostedPaymentSettings")]
    public HostedPaymentSettings HostedPaymentSettings { get; set; } = new();
}
