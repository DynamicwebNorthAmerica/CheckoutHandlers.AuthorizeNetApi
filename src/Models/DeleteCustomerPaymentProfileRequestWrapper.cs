using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class DeleteCustomerPaymentProfileRequestWrapper
{
    [JsonPropertyName("deleteCustomerPaymentProfileRequest"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DeleteCustomerPaymentProfileRequest? DeleteCustomerPaymentProfileRequest { get; set; }
}
