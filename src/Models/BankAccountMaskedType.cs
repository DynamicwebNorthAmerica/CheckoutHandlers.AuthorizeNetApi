using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class BankAccountMaskedType
{
    [JsonPropertyName("accountType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public BankAccountTypeEnum? AccountType { get; set; }

    [JsonPropertyName("echeckType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EcheckTypeEnum? EcheckType { get; set; }

    [JsonPropertyName("routingNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RoutingNumber { get; set; }

    [JsonPropertyName("accountNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("nameOnAccount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? NameOnAccount { get; set; }

    [JsonPropertyName("bankName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BankName { get; set; }
}
