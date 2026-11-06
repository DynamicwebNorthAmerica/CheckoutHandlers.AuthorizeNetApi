using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class BatchDetailsType
{
    [JsonPropertyName("batchId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BatchId { get; set; }

    [JsonPropertyName("settlementTimeUTC"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SettlementTimeUTC { get; set; }

    [JsonPropertyName("settlementTimeLocal"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SettlementTimeLocal { get; set; }

    [JsonPropertyName("settlementState"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SettlementStateEnum? SettlementState { get; set; }

    [JsonPropertyName("paymentMethod"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PaymentMethodEnum? PaymentMethod { get; set; }

    [JsonPropertyName("marketType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? MarketType { get; set; }

    [JsonPropertyName("product"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Product { get; set; }

    [JsonPropertyName("statistics"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<BatchStatisticType>? Statistics { get; set; }
}


