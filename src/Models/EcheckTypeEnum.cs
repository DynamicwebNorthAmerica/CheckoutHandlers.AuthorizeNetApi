using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<EcheckTypeEnum>))]
internal enum EcheckTypeEnum
{
    [EnumMember(Value = "PPD")]
    PPD,

    [EnumMember(Value = "WEB")]
    WEB,

    [EnumMember(Value = "CCD")]
    CCD,

    [EnumMember(Value = "TEL")]
    TEL,

    [EnumMember(Value = "ARC")]
    ARC,

    [EnumMember(Value = "BOC")]
    BOC
}