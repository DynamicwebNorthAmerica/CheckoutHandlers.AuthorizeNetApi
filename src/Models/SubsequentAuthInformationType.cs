using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class SubsequentAuthInformationType
{
    [JsonPropertyName("originalNetworkTransId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? OriginalNetworkTransId { get; set; }

    private double _originalAuthAmount;

    [JsonPropertyName("originalAuthAmount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double OriginalAuthAmount
    {
        get => _originalAuthAmount;
        set => _originalAuthAmount = AmountHelper.AdjustAmount(value);
    }

    [JsonPropertyName("reason"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Reason { get; set; }
}
