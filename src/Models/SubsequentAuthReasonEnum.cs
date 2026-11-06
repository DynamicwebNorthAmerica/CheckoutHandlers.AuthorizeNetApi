using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<SubsequentAuthReasonEnum>))]
internal enum SubsequentAuthReasonEnum
{
    [EnumMember(Value = "resubmission")]
    Resubmission,

    [EnumMember(Value = "delayedCharge")]
    DelayedCharge,

    [EnumMember(Value = "reauthorization")]
    Reauthorization,

    [EnumMember(Value = "noShow")]
    NoShow
}