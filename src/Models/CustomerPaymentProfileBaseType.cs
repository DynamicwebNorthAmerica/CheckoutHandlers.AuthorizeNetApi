using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerPaymentProfileBaseType
{
    [JsonPropertyName("customerType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerTypeEnum? CustomerType { get; set; }

    [JsonPropertyName("billTo"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CustomerAddressType? BillTo { get; set; }
}
