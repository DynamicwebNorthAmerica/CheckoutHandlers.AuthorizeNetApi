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
    /// Prefix of the refId built from the order AutoId. It keeps the refId apart from plain numbers such as
    /// invoice numbers of transactions created outside Dynamicweb.
    /// </summary>
    public const string AutoIdRefIdPrefix = "DW-";

    /// <summary>
    /// The Authorize.Net refId for an order. It comes back as merchantReferenceId in webhook notifications
    /// and is used to find the order again. The AutoId is used because Order.Id changes from the cart id to
    /// the order id when the order completes, while AutoId never changes.
    /// </summary>
    public static string? GetRefId(Order order)
        => order.AutoId > 0 ? GetRefId(AutoIdRefIdPrefix + order.AutoId) : GetRefId(order.Id);

    /// <summary>
    /// Reads the AutoId back from a refId built by <see cref="GetRefId(Order)"/>.
    /// </summary>
    public static bool TryParseAutoIdRefId(string? refId, out int autoId)
    {
        autoId = 0;
        return refId is not null
            && refId.StartsWith(AutoIdRefIdPrefix, StringComparison.Ordinal)
            && int.TryParse(refId.AsSpan(AutoIdRefIdPrefix.Length), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out autoId)
            && autoId > 0;
    }

    /// <summary>
    /// True when an Authorize.Net transaction belongs to the order's payment: it is the payment transaction
    /// itself (auth, capture, void) or refers to it (refund). Before the order has a transaction number, the
    /// transaction must carry the order id as invoice number.
    /// </summary>
    /// <remarks>
    /// Webhooks arrive for every transaction on the merchant account, including ones created outside this
    /// site, so the order found from the notification is only trusted after this check.
    /// </remarks>
    public static bool IsTransactionForOrder(Order order, string? transId, string? refTransId, string? invoiceNumber)
        => IsTransactionForOrder(order.TransactionNumber, order.Id, transId, refTransId, invoiceNumber);

    internal static bool IsTransactionForOrder(string? orderTransactionNumber, string? orderId, string? transId, string? refTransId, string? invoiceNumber)
    {
        if (!string.IsNullOrWhiteSpace(orderTransactionNumber))
        {
            return string.Equals(transId, orderTransactionNumber, StringComparison.Ordinal)
                || string.Equals(refTransId, orderTransactionNumber, StringComparison.Ordinal);
        }

        return !string.IsNullOrWhiteSpace(invoiceNumber)
            && !string.IsNullOrWhiteSpace(orderId)
            && string.Equals(invoiceNumber.Trim(), orderId, StringComparison.OrdinalIgnoreCase);
    }

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
