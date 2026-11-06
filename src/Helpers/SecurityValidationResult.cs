using System.Collections.Generic;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Result of security configuration validation
/// </summary>
internal sealed class SecurityValidationResult
{
    public bool IsValid { get; init; }
    public List<string> SecurityIssues { get; init; } = [];
}
