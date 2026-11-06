using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<CustomerProfileTypeEnum>))]
internal enum CustomerProfileTypeEnum
{
    [EnumMember(Value = "regular")]
    Regular,

    [EnumMember(Value = "guest")]
    Guest
}