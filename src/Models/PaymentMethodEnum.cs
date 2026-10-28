using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<PaymentMethodEnum>))]
internal enum PaymentMethodEnum
{
    [EnumMember(Value = "creditCard")]
    CreditCard,

    [EnumMember(Value = "eCheck")]
    ECheck,

    [EnumMember(Value = "payPal")]
    PayPal
}