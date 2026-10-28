using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class BatchStatisticType
{
    [JsonPropertyName("accountType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public AccountTypeEnum? AccountType { get; set; }

    [JsonPropertyName("chargeAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ChargeAmount { get; set; }

    [JsonPropertyName("chargeCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ChargeCount { get; set; }

    [JsonPropertyName("refundAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double RefundAmount { get; set; }

    [JsonPropertyName("refundCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int RefundCount { get; set; }

    [JsonPropertyName("voidCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int VoidCount { get; set; }

    [JsonPropertyName("declineCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int DeclineCount { get; set; }

    [JsonPropertyName("errorCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorCount { get; set; }

    [JsonPropertyName("returnedItemAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ReturnedItemAmount { get; set; }

    [JsonPropertyName("returnedItemCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ReturnedItemCount { get; set; }

    [JsonPropertyName("chargebackAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ChargebackAmount { get; set; }

    [JsonPropertyName("chargebackCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ChargebackCount { get; set; }

    [JsonPropertyName("correctionNoticeCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int CorrectionNoticeCount { get; set; }

    [JsonPropertyName("chargeChargeBackAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ChargeChargeBackAmount { get; set; }

    [JsonPropertyName("chargeChargeBackCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ChargeChargeBackCount { get; set; }

    [JsonPropertyName("refundChargeBackAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double RefundChargeBackAmount { get; set; }

    [JsonPropertyName("refundChargeBackCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int RefundChargeBackCount { get; set; }

    [JsonPropertyName("chargeReturnedItemsAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ChargeReturnedItemsAmount { get; set; }

    [JsonPropertyName("chargeReturnedItemsCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ChargeReturnedItemsCount { get; set; }

    [JsonPropertyName("refundReturnedItemsAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double RefundReturnedItemsAmount { get; set; }

    [JsonPropertyName("refundReturnedItemsCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int RefundReturnedItemsCount { get; set; }
}
