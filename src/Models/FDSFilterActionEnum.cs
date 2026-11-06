using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<FdsFilterActionEnum>))]
internal enum FdsFilterActionEnum
{
    [EnumMember(Value = "reject")]
    Reject,

    [EnumMember(Value = "decline")]
    Decline,

    [EnumMember(Value = "hold")]
    Hold,

    [EnumMember(Value = "authAndHold")]
    AuthAndHold,

    [EnumMember(Value = "report")]
    Report
}