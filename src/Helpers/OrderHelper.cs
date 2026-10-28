using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.Orders;
using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class OrderHelper
{
    /// <summary>
    /// Tolerance used when comparing money amounts held as doubles.
    /// </summary>
    public const double AmountTolerance = 0.01;

    public static string GetOrderError(Order? order)
    {
        if (order is null)
            return PreparedMessages.OrderNotSetMessage;

        if (string.IsNullOrEmpty(order.Id))
            return PreparedMessages.OrderIdNotSetMessage;

        if (string.IsNullOrEmpty(order.TransactionNumber))
            return PreparedMessages.TransactionNumberNotSetMessage;

        return string.Empty;
    }

    public static void UpdateTransactionNumber(Order order, string? transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId) || transactionId.Equals("0", StringComparison.OrdinalIgnoreCase)
            || transactionId.Equals(order.TransactionNumber, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        order.TransactionNumber = transactionId;
    }

    /// <summary>
    /// The amount sent to Authorize.Net: order total including the payment fee, rounded to the currency.
    /// </summary>
    public static double GetOrderAmount(Order order)
        => Ecommerce.Services.Currencies.Round(order.Currency, order.Price.Price + order.ExternalPaymentFee);

    /// <summary>
    /// The Authorize.Net refId for an order. It comes back as merchantReferenceId in webhook notifications
    /// and is used to find the order again. The numeric AutoId is used because Order.Id changes from the
    /// cart id to the order id when the order completes, while AutoId never changes.
    /// </summary>
    public static string? GetRefId(Order order)
        => order.AutoId > 0 ? GetRefId(order.AutoId.ToString()) : GetRefId(order.Id);

    /// <summary>
    /// Returns the reference as an Authorize.Net refId when it fits the 20 character limit, otherwise null.
    /// </summary>
    public static string? GetRefId(string? reference)
        => string.IsNullOrWhiteSpace(reference) || reference.Length > SecuritySettings.RefIdMaxLength
            ? null
            : reference;

    public static bool AmountsEqual(double left, double right)
        => Math.Abs(left - right) <= AmountTolerance;
}
