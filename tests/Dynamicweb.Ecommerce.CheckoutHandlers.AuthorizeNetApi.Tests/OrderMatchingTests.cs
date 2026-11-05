using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

/// <summary>
/// How webhook notifications are matched to orders. Webhooks arrive for every transaction on the merchant
/// account, so a notification must never be applied to an order its transaction does not belong to.
/// </summary>
public class OrderMatchingTests
{
    [Theory]
    [InlineData("DW-42", true, 42)]
    [InlineData("DW-1", true, 1)]
    [InlineData("42", false, 0)]          // plain numbers (invoice numbers of other systems) are not AutoIds
    [InlineData("DW-0", false, 0)]
    [InlineData("DW--5", false, 0)]
    [InlineData("DW-+5", false, 0)]
    [InlineData("DW-5x", false, 0)]
    [InlineData("dw-5", false, 0)]
    [InlineData("DW-", false, 0)]
    [InlineData("ORDER42", false, 0)]
    [InlineData(null, false, 0)]
    public void Only_prefixed_ref_ids_are_read_as_auto_ids(string? refId, bool expected, int expectedAutoId)
    {
        Assert.Equal(expected, OrderHelper.TryParseAutoIdRefId(refId, out int autoId));
        Assert.Equal(expectedAutoId, autoId);
    }

    [Fact]
    public void Prefixed_ref_id_fits_the_authorize_net_limit_for_any_auto_id()
    {
        Assert.True((OrderHelper.AutoIdRefIdPrefix + int.MaxValue).Length <= 20);
    }

    [Theory]
    // The order has a transaction: the notification's transaction must be it, or refer to it.
    [InlineData("6001", "CART1", "6001", null, "CART1", true)]     // auth, capture, void of the payment
    [InlineData("6001", "ORDER9", "7002", "6001", null, true)]     // refund of the payment
    [InlineData("6001", "ORDER9", "7002", null, "ORDER9", false)]  // same invoice number, other transaction
    [InlineData("6001", "ORDER9", "7002", "6999", null, false)]    // refund of another transaction
    // No transaction yet (webhook before the customer returned): the invoice number must be the order id.
    [InlineData("", "CART1", "6001", null, "CART1", true)]
    [InlineData(null, "CART1", "6001", null, " cart1 ", true)]
    [InlineData(null, "CART1", "6001", null, "CART2", false)]
    [InlineData(null, "CART1", "6001", null, null, false)]
    [InlineData(null, "", "6001", null, "", false)]
    public void A_transaction_belongs_to_an_order_only_through_its_transaction_number_or_invoice_number(
        string? orderTransactionNumber, string? orderId, string? transId, string? refTransId, string? invoiceNumber, bool expected)
    {
        Assert.Equal(expected, OrderHelper.IsTransactionForOrder(orderTransactionNumber, orderId, transId, refTransId, invoiceNumber));
    }
}
