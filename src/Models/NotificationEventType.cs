using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<NotificationEventType>))]
internal enum NotificationEventType
{
    /// <summary>
    /// Notifies you that an authorization and capture transaction was created.
    /// </summary>
    [EnumMember(Value = "AuthCaptureCreated")]
    AuthCaptureCreated,

    /// <summary>
    /// Notifies you that an authorization transaction was created.
    /// </summary>
    [EnumMember(Value = "AuthCreated")]
    AuthCreated,

    /// <summary>
    /// Notifies you that a capture transaction was created.
    /// </summary>
    [EnumMember(Value = "CaptureCreated")]
    CaptureCreated,

    /// <summary>
    /// Notifies you that a previous authorization was captured.
    /// </summary>
    [EnumMember(Value = "PriorAuthCaptureCreated")]
    PriorAuthCaptureCreated,

    /// <summary>
    /// Notifies you that a successfully settled transaction was refunded.
    /// </summary>
    [EnumMember(Value = "RefundCreated")]
    RefundCreated,

    /// <summary>
    /// Notifies you that an unsettled transaction was voided.
    /// </summary>
    [EnumMember(Value = "VoidCreated")]
    VoidCreated,

    /// <summary>
    /// Notifies you that a transaction held for review was approved by the merchant.
    /// </summary>
    [EnumMember(Value = "FraudApproved")]
    FraudApproved,

    /// <summary>
    /// Notifies you that a transaction held for review was declined by the merchant.
    /// </summary>
    [EnumMember(Value = "FraudDeclined")]
    FraudDeclined,

    /// <summary>
    /// Notifies you that a transaction was held for review by the fraud filters.
    /// </summary>
    [EnumMember(Value = "FraudHeld")]
    FraudHeld
}
