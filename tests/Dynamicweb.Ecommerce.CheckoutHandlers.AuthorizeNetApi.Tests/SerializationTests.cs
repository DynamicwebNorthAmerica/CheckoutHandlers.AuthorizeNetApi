using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

/// <summary>
/// Request and response shapes against the Authorize.Net JSON API.
/// </summary>
public class SerializationTests
{
    [Fact]
    public void Unsettled_transaction_list_request_has_the_documented_shape()
    {
        var wrapper = new GetUnsettledTransactionListRequestWrapper
        {
            GetUnsettledTransactionListRequest = new GetUnsettledTransactionListRequest
            {
                MerchantAuthentication = new MerchantAuthenticationType { Name = "login", TransactionKey = "key" },
                Sorting = new TransactionListSorting(),
                Paging = new TransactionListPaging { Limit = 1000, Offset = 1 }
            }
        };

        string json = AuthorizeNetJson.Serialize(wrapper);

        Assert.Contains("\"getUnsettledTransactionListRequest\":{", json);
        Assert.Contains("\"merchantAuthentication\":{\"name\":\"login\",\"transactionKey\":\"key\"}", json);
        Assert.Contains("\"sorting\":{\"orderBy\":\"submitTimeUTC\",\"orderDescending\":true}", json);
        Assert.Contains("\"paging\":{\"limit\":1000,\"offset\":1}", json);
        Assert.DoesNotContain("\"status\"", json);
        Assert.DoesNotContain("\"refId\"", json);
    }

    [Fact]
    public void Unsettled_transaction_list_response_is_read()
    {
        const string json = """
            {"transactions":[{"transId":"60020981676","submitTimeUTC":"2026-10-28T10:15:30Z","submitTimeLocal":"2026-10-28T03:15:30","transactionStatus":"authorizedPendingCapture","invoiceNumber":"ORDER1042","firstName":"Jane","lastName":"Doe","accountType":"Visa","accountNumber":"XXXX1111","settleAmount":45.00,"marketType":"eCommerce","product":"Card Not Present"}],"totalNumInResultSet":1,"messages":{"resultCode":"Ok","message":[{"code":"I00001","text":"Successful."}]}}
            """;

        GetUnsettledTransactionListResponse? response = AuthorizeNetJson.Deserialize<GetUnsettledTransactionListResponse>(json);

        Assert.NotNull(response);
        Assert.True(AuthorizeNetService.IsOk(response.Messages));
        TransactionSummaryType transaction = Assert.Single(response.Transactions!);
        Assert.Equal("60020981676", transaction.TransId);
        Assert.Equal("ORDER1042", transaction.InvoiceNumber);
        Assert.Equal(TransactionStatusEnum.AuthorizedPendingCapture, transaction.TransactionStatus);
        Assert.Equal("XXXX1111", transaction.AccountNumber);
        Assert.Equal(45.00, transaction.SettleAmount);
    }

    [Fact]
    public void Create_transaction_request_includes_refId_and_invoice_number()
    {
        var wrapper = new CreateTransactionRequestWrapper
        {
            CreateTransactionRequest = new CreateTransactionRequest
            {
                MerchantAuthentication = new MerchantAuthenticationType { Name = "login", TransactionKey = "key" },
                RefId = "ORDER1042",
                TransactionRequest = new TransactionRequestType
                {
                    TransactionType = TransactionTypeEnum.VoidTransaction,
                    RefTransId = "60020981676",
                    Order = new OrderType { InvoiceNumber = "ORDER1042" }
                }
            }
        };

        string json = AuthorizeNetJson.Serialize(wrapper);

        Assert.Contains("\"refId\":\"ORDER1042\"", json);
        Assert.Contains("\"transactionType\":\"voidTransaction\"", json);
        Assert.Contains("\"refTransId\":\"60020981676\"", json);
        Assert.Contains("\"order\":{\"invoiceNumber\":\"ORDER1042\"}", json);
        Assert.DoesNotContain("\"amount\"", json);
    }

    [Fact]
    public void Error_envelope_is_detected()
    {
        const string json = """{"messages":{"resultCode":"Error","message":[{"code":"E00007","text":"User authentication failed due to invalid authentication values."}]}}""";

        ErrorResponse? error = AuthorizeNetHttpService.TryParseError(json);

        Assert.NotNull(error);
        Assert.Equal("E00007", error.Messages?.Message?[0].Code);
    }

    [Fact]
    public void Ok_envelope_is_not_an_error()
    {
        const string json = """{"transactionResponse":{"responseCode":"1","transId":"60020981676"},"messages":{"resultCode":"Ok","message":[{"code":"I00001","text":"Successful."}]}}""";

        Assert.Null(AuthorizeNetHttpService.TryParseError(json));

        CreateTransactionResponse? response = AuthorizeNetJson.Deserialize<CreateTransactionResponse>(json);
        Assert.Equal("60020981676", response?.TransactionResponse?.TransId);
    }

    [Fact]
    public void Transaction_details_with_unknown_status_do_not_fail()
    {
        const string json = """{"transaction":{"transId":"1","transactionType":"authCaptureTransaction","transactionStatus":"brandNewStatus","responseCode":1,"authAmount":10.0,"settleAmount":10.0,"payment":{"creditCard":{"cardNumber":"XXXX1111","expirationDate":"XXXX","cardType":"Visa"}}},"messages":{"resultCode":"Ok"}}""";

        GetTransactionDetailsResponse? response = AuthorizeNetJson.Deserialize<GetTransactionDetailsResponse>(json);

        Assert.NotNull(response?.Transaction);
        Assert.Equal(TransactionStatusEnum.Unknown, response.Transaction.TransactionStatus);
        Assert.Equal(TransactionTypeEnum.AuthCaptureTransaction, response.Transaction.TransactionType);
        Assert.Equal("XXXX1111", response.Transaction.Payment?.CreditCard?.CardNumber);
    }
}
