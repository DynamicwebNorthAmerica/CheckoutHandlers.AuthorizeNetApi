using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Masks secrets and card data before anything is written to a log.
/// </summary>
internal static partial class SecurityHelper
{
    [GeneratedRegex(@"(cardNumber[""']?[:\s=]+[""']?)(\d{13,19})([""']?)", RegexOptions.IgnoreCase)]
    private static partial Regex CardNumberPattern();

    [GeneratedRegex(@"(cardCode[""']?[:\s=]+[""']?)(\d{3,4})([""']?)", RegexOptions.IgnoreCase)]
    private static partial Regex CardCodePattern();

    [GeneratedRegex(@"(transactionKey[""']?[:\s=]+[""']?)([^""'\s,}]+)([""']?)", RegexOptions.IgnoreCase)]
    private static partial Regex TransactionKeyPattern();

    [GeneratedRegex(@"(password[""']?[:\s=]+[""']?)([^""'\s,}]+)([""']?)", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordPattern();

    [GeneratedRegex(@"(authorization[""']?\s*[:=]\s*)(basic|bearer)(\s+)(\S+)", RegexOptions.IgnoreCase)]
    private static partial Regex AuthorizationHeaderPattern();

    /// <summary>
    /// Masks a card number for logging, keeping only the last four digits.
    /// </summary>
    public static string MaskCreditCardNumber(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return "";

        string cleaned = cardNumber.Replace(" ", "").Replace("-", "");

        if (cleaned.Length < 4)
            return new string('X', cleaned.Length);

        if (cleaned.Length <= 8)
            return new string('X', cleaned.Length - 4) + cleaned[^4..];

        return SecuritySettings.CardNumberMask + cleaned[^4..];
    }

    /// <summary>
    /// Masks sensitive values in request/response text before logging:
    /// card numbers, card codes, transaction keys, passwords and HTTP Authorization credentials.
    /// </summary>
    public static string MaskSensitiveDataInContent(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return "";

        string masked = content;

        masked = CardNumberPattern().Replace(masked,
            match => match.Groups[1].Value + MaskCreditCardNumber(match.Groups[2].Value) + match.Groups[3].Value);

        masked = CardCodePattern().Replace(masked,
            match => match.Groups[1].Value + "XXX" + match.Groups[3].Value);

        masked = TransactionKeyPattern().Replace(masked,
            match => match.Groups[1].Value + SecuritySettings.SecretMask + match.Groups[3].Value);

        masked = PasswordPattern().Replace(masked,
            match => match.Groups[1].Value + SecuritySettings.SecretMask + match.Groups[3].Value);

        masked = AuthorizationHeaderPattern().Replace(masked,
            match => match.Groups[1].Value + match.Groups[2].Value + match.Groups[3].Value + SecuritySettings.SecretMask);

        return masked;
    }

    /// <summary>
    /// Validates that the credentials needed to talk to Authorize.Net and to verify webhooks are configured.
    /// </summary>
    public static SecurityValidationResult ValidateSecurityConfiguration(string? apiLoginId, string? transactionKey, string? signatureKey)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(apiLoginId))
            issues.Add("API Login ID is required");

        if (string.IsNullOrWhiteSpace(transactionKey))
            issues.Add("Transaction Key is required");

        if (string.IsNullOrWhiteSpace(signatureKey))
            issues.Add("Signature Key is required");

        return new SecurityValidationResult
        {
            IsValid = issues.Count == 0,
            SecurityIssues = issues
        };
    }
}
