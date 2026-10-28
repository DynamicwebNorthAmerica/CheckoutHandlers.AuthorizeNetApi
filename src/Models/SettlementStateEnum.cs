using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<SettlementStateEnum>))]
internal enum SettlementStateEnum
{
    [EnumMember(Value = "settledSuccessfully")]
    SettledSuccessfully,

    [EnumMember(Value = "settlementError")]
    SettlementError,

    [EnumMember(Value = "pendingSettlement")]
    PendingSettlement
}