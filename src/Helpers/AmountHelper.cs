using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Amount handling for the Authorize.Net API, which accepts up to four decimals.
/// </summary>
internal static class AmountHelper
{
    private const int ApiDecimals = 4;

    /// <summary>
    /// Rounds an amount to the four decimals the API accepts, removing floating point noise.
    /// </summary>
    public static double AdjustAmount(double amount) => Math.Round(amount, ApiDecimals, MidpointRounding.AwayFromZero);
}
