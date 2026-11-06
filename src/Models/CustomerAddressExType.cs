using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class CustomerAddressExType
{
    [JsonPropertyName("firstName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? LastName { get; set; }

    [JsonPropertyName("company"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Company { get; set; }

    [JsonPropertyName("address"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Address { get; set; }

    [JsonPropertyName("city"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? City { get; set; }

    [JsonPropertyName("state"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? State { get; set; }

    [JsonPropertyName("zip"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Zip { get; set; }

    [JsonPropertyName("country"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Country { get; set; }

    [JsonPropertyName("phoneNumber"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("email"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Email { get; set; }

    [JsonPropertyName("customerAddressId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CustomerAddressId { get; set; }
}


