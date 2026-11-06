using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<TransactionTypeEnum>))]
internal enum TransactionTypeEnum
{
    /// <summary>
    /// A value this handler does not know. Set by the JSON converter when Authorize.Net returns a new value.
    /// </summary>
    Unknown = 0,

    [EnumMember(Value = "authOnlyTransaction")]
    AuthOnlyTransaction,

    [EnumMember(Value = "authCaptureTransaction")]
    AuthCaptureTransaction,

    [EnumMember(Value = "captureOnlyTransaction")]
    CaptureOnlyTransaction,

    [EnumMember(Value = "refundTransaction")]
    RefundTransaction,

    [EnumMember(Value = "priorAuthCaptureTransaction")]
    PriorAuthCaptureTransaction,

    [EnumMember(Value = "voidTransaction")]
    VoidTransaction,

    [EnumMember(Value = "getDetailsTransaction")]
    GetDetailsTransaction,

    [EnumMember(Value = "authOnlyContinueTransaction")]
    AuthOnlyContinueTransaction,

    [EnumMember(Value = "authCaptureContinueTransaction")]
    AuthCaptureContinueTransaction
}