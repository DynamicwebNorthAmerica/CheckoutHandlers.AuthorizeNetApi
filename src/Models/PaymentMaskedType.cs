using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class PaymentMaskedType
{
    [JsonPropertyName("creditCard"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CreditCardMaskedType? CreditCard { get; set; }

    [JsonPropertyName("bankAccount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public BankAccountMaskedType? BankAccount { get; set; }

    [JsonPropertyName("tokenInformation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TokenMaskedType? TokenInformation { get; set; }
}
