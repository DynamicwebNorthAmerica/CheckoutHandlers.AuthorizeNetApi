using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// JSON converter for enums that honours <see cref="EnumMemberAttribute"/> values.
/// Unknown values from the API map to a member named "Unknown" when the enum declares one,
/// otherwise to the enum default, so that a new value introduced by Authorize.Net never breaks deserialization.
/// </summary>
/// <typeparam name="T">The enum type to convert</typeparam>
internal sealed class DataContractEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private const string UnknownMemberName = "Unknown";

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();

            if (TryParse(stringValue, out T parsed))
                return parsed;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue) && Enum.IsDefined(typeof(T), intValue))
                return (T)Enum.ToObject(typeof(T), intValue);
        }

        return GetFallbackValue();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToEnumMemberValue());

    /// <summary>
    /// Parses an API string value by EnumMember value first, then by member name (case-insensitive).
    /// </summary>
    public static bool TryParse(string? stringValue, out T result)
    {
        result = default;
        if (string.IsNullOrEmpty(stringValue))
            return false;

        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            EnumMemberAttribute? enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
            if (string.Equals(enumMember?.Value, stringValue, StringComparison.Ordinal))
            {
                result = (T)field.GetValue(null)!;
                return true;
            }
        }

        return Enum.TryParse(stringValue, true, out result) && Enum.IsDefined(typeof(T), result);
    }

    private static T GetFallbackValue()
    {
        FieldInfo? unknown = typeof(T).GetField(UnknownMemberName, BindingFlags.Public | BindingFlags.Static);
        return unknown is not null ? (T)unknown.GetValue(null)! : default;
    }
}
