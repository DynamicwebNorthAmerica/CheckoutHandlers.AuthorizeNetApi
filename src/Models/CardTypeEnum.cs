using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<CardTypeEnum>))]
internal enum CardTypeEnum
{
    [EnumMember(Value = "Visa")]
    Visa,

    [EnumMember(Value = "MasterCard")]
    MasterCard,

    [EnumMember(Value = "AmericanExpress")]
    AmericanExpress,

    [EnumMember(Value = "Discover")]
    Discover,

    [EnumMember(Value = "JCB")]
    JCB,

    [EnumMember(Value = "DinersClub")]
    DinersClub
}