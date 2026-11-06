using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class Setting
{
    [JsonPropertyName("settingName"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SettingName { get; set; }

    [JsonPropertyName("settingValue"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SettingValue { get; set; }
}
