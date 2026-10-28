using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<TransactionStatusEnum>))]
internal enum TransactionStatusEnum
{
    /// <summary>
    /// A value this handler does not know. Set by the JSON converter when Authorize.Net returns a new value.
    /// </summary>
    Unknown = 0,

    [EnumMember(Value = "authorizedPendingCapture")]
    AuthorizedPendingCapture,

    [EnumMember(Value = "capturedPendingSettlement")]
    CapturedPendingSettlement,

    [EnumMember(Value = "communicationError")]
    CommunicationError,

    [EnumMember(Value = "refundSettledSuccessfully")]
    RefundSettledSuccessfully,

    [EnumMember(Value = "refundPendingSettlement")]
    RefundPendingSettlement,

    [EnumMember(Value = "approvedReview")]
    ApprovedReview,

    [EnumMember(Value = "declined")]
    Declined,

    [EnumMember(Value = "couldNotVoid")]
    CouldNotVoid,

    [EnumMember(Value = "expired")]
    Expired,

    [EnumMember(Value = "generalError")]
    GeneralError,

    [EnumMember(Value = "pendingFinalSettlement")]
    PendingFinalSettlement,

    [EnumMember(Value = "pendingSettlement")]
    PendingSettlement,

    [EnumMember(Value = "failedReview")]
    FailedReview,

    [EnumMember(Value = "settledSuccessfully")]
    SettledSuccessfully,

    [EnumMember(Value = "settlementError")]
    SettlementError,

    [EnumMember(Value = "underReview")]
    UnderReview,

    [EnumMember(Value = "updatingSettlement")]
    UpdatingSettlement,

    [EnumMember(Value = "voided")]
    Voided,

    [EnumMember(Value = "FDSPendingReview")]
    FDSPendingReview,

    [EnumMember(Value = "FDSAuthorizedPendingReview")]
    FDSAuthorizedPendingReview,

    [EnumMember(Value = "returnedItem")]
    ReturnedItem,

    [EnumMember(Value = "chargeback")]
    Chargeback,

    [EnumMember(Value = "chargebackReversal")]
    ChargebackReversal,

    [EnumMember(Value = "authorizedPendingRelease")]
    AuthorizedPendingRelease
}