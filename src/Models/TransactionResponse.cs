using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class TransactionResponse
{
    [JsonPropertyName("responseCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ResponseCode { get; set; }

    [JsonPropertyName("rawResponseCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RawResponseCode { get; set; }

    [JsonPropertyName("authCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AuthCode { get; set; }

    [JsonPropertyName("avsResultCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AvsResultCode { get; set; }

    [JsonPropertyName("cvvResultCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CvvResultCode { get; set; }

    [JsonPropertyName("cavvResultCode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CavvResultCode { get; set; }

    [JsonPropertyName("transId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TransId { get; set; }

    [JsonPropertyName("refTransID"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RefTransId { get; set; }

    [JsonPropertyName("testRequest"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TestRequest { get; set; }

    [JsonPropertyName("accountNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("entryMode"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EntryMode { get; set; }

    [JsonPropertyName("accountType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AccountType { get; set; }

    [JsonPropertyName("splitTenderId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SplitTenderId { get; set; }

    [JsonPropertyName("prePaidCard"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PrePaidCard? PrePaidCard { get; set; }

    [JsonPropertyName("messages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<Message>? Messages { get; set; }

    [JsonPropertyName("errors"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<Error>? Errors { get; set; }

    [JsonPropertyName("splitTenderPayments"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<SplitTenderPayment>? SplitTenderPayments { get; set; }

    [JsonPropertyName("userFields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<UserField>? UserFields { get; set; }

    [JsonPropertyName("shipTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public NameAndAddressType? ShipTo { get; set; }

    [JsonPropertyName("secureAcceptance"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SecureAcceptance? SecureAcceptance { get; set; }

    [JsonPropertyName("emvResponse"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EmvResponse? EmvResponse { get; set; }

    [JsonPropertyName("transHashSha2"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? TransHashSha2 { get; set; }

    [JsonPropertyName("profile"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerProfileIdType? Profile { get; set; }

    [JsonPropertyName("networkTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? NetworkTransId { get; set; }
}
