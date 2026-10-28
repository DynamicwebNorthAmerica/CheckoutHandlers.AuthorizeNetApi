using System;
using System.Security.Cryptography;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Validates the X-ANET-Signature header Authorize.Net sends with every webhook notification.
/// The signature is HMAC-SHA512 over the raw request body, keyed with the merchant Signature Key,
/// sent as "sha512=&lt;hex&gt;".
/// </summary>
internal static class HmacValidator
{
    private const string SignaturePrefix = "sha512=";

    /// <summary>
    /// Returns true when <paramref name="incomingSignature"/> matches the HMAC-SHA512 of <paramref name="notificationBody"/>.
    /// The optional "sha512=" prefix is accepted. The comparison runs in constant time.
    /// </summary>
    public static bool IsValid(string? signatureKey, string? notificationBody, string? incomingSignature)
    {
        if (string.IsNullOrWhiteSpace(signatureKey) || string.IsNullOrEmpty(notificationBody) || string.IsNullOrWhiteSpace(incomingSignature))
            return false;

        string hex = incomingSignature.Trim();
        if (hex.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
            hex = hex.Substring(SignaturePrefix.Length);

        byte[] incoming;
        try
        {
            incoming = Convert.FromHexString(hex);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] expected = ComputeSignature(signatureKey.Trim(), notificationBody);

        return CryptographicOperations.FixedTimeEquals(expected, incoming);
    }

    /// <summary>
    /// Computes the raw HMAC-SHA512 signature bytes.
    /// </summary>
    public static byte[] ComputeSignature(string signatureKey, string notificationBody)
    {
        byte[] key = Encoding.UTF8.GetBytes(signatureKey);
        return HMACSHA512.HashData(key, Encoding.UTF8.GetBytes(notificationBody));
    }

    /// <summary>
    /// Computes the signature as Authorize.Net sends it: upper-case hex, no prefix.
    /// </summary>
    public static string ComputeSignatureHex(string signatureKey, string notificationBody)
        => Convert.ToHexString(ComputeSignature(signatureKey, notificationBody));
}
