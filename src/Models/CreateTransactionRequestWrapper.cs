using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CreateTransactionRequestWrapper
{
    [JsonPropertyName("createTransactionRequest")]
    public CreateTransactionRequest CreateTransactionRequest { get; set; } = new();
}