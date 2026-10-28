using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<ValidationModeEnum>))]
internal enum ValidationModeEnum
{
    [EnumMember(Value = "none")]
    None,

    [EnumMember(Value = "testMode")]
    TestMode,

    [EnumMember(Value = "liveMode")]
    LiveMode,

    [EnumMember(Value = "oldLiveMode")]
    OldLiveMode
}