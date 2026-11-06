using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

/// <summary>
/// Runs the service against a stub HTTP handler: what is sent, and how answers and error envelopes are read.
/// </summary>
[Collection(ServiceRoundTripCollection.Name)]
public sealed class ServiceRoundTripTests : IDisposable
{
    private readonly StubHttpMessageHandler _handler = new();

    public ServiceRoundTripTests()
    {
        AuthorizeNetHttpService.ClientOverride = new HttpClient(_handler) { BaseAddress = new Uri("https://apitest.authorize.net/") };
    }

    public void Dispose() => AuthorizeNetHttpService.ClientOverride = null;

    private static AuthorizeNetService CreateService() => new("login", "key", isTestMode: true, debugLogging: false, order: null);

    private const string OkMessages = "\"messages\":{\"resultCode\":\"Ok\",\"message\":[{\"code\":\"I00001\",\"text\":\"Successful.\"}]}";

    [Fact]
    public void Unsettled_lookup_reads_one_page_of_1000_newest_first_and_matches_the_invoice_number()
    {
        _handler.Enqueue($$"""
            {"transactions":[
              {"transId":"111","invoiceNumber":"CART7"},
              {"transId":"222","invoiceNumber":" cart42 "}
            ],{{OkMessages}}}
            """);

        TransactionSummaryType? match = CreateService().FindUnsettledTransaction("CART42");

        Assert.Equal("222", match?.TransId);
        JsonElement request = JsonDocument.Parse(Assert.Single(_handler.RequestBodies)).RootElement.GetProperty("getUnsettledTransactionListRequest");
        Assert.Equal(1000, request.GetProperty("paging").GetProperty("limit").GetInt32());
        Assert.Equal(1, request.GetProperty("paging").GetProperty("offset").GetInt32());
        Assert.True(request.GetProperty("sorting").GetProperty("orderDescending").GetBoolean());
    }

    [Fact]
    public void Unsettled_lookup_returns_null_when_no_invoice_matches()
    {
        _handler.Enqueue($$"""{"transactions":[{"transId":"111","invoiceNumber":"CART7"}],{{OkMessages}}}""");

        Assert.Null(CreateService().FindUnsettledTransaction("CART42"));
    }

    [Fact]
    public void Transaction_details_api_disabled_names_the_setting()
    {
        _handler.Enqueue("""
            {"messages":{"resultCode":"Error","message":[{"code":"E00011","text":"Access denied. You do not have permissions to call the Transaction Details API."}]}}
            """);

        var exception = Assert.Throws<AuthorizeNetApiException>(() => CreateService().GetTransactionDetails("123"));

        Assert.Contains("[E00011]", exception.Message);
        Assert.Contains(AuthorizeNetHttpService.TransactionDetailsApiDisabledHint, exception.Message);
    }

    [Fact]
    public void Other_api_errors_do_not_get_the_transaction_details_hint()
    {
        _handler.Enqueue("""{"messages":{"resultCode":"Error","message":[{"code":"E00007","text":"User authentication failed due to invalid authentication values."}]}}""");

        var exception = Assert.Throws<AuthorizeNetApiException>(() => CreateService().GetTransactionDetails("123"));

        Assert.Contains("[E00007]", exception.Message);
        Assert.DoesNotContain(AuthorizeNetHttpService.TransactionDetailsApiDisabledCode + "]", exception.Message);
        Assert.DoesNotContain("Transaction Details API in the", exception.Message);
    }

    [Fact]
    public void Transaction_details_are_read_with_ref_trans_id_and_invoice_number()
    {
        _handler.Enqueue($$"""
            {"transaction":{"transId":"60123","refTransId":"60100","responseCode":1,"order":{"invoiceNumber":"ORDER5"},"settleAmount":12.5},{{OkMessages}}}
            """);

        TransactionDetailsType details = CreateService().GetTransactionDetails("60123");

        Assert.Equal("60123", details.TransId);
        Assert.Equal("60100", details.RefTransId);
        Assert.Equal("ORDER5", details.Order?.InvoiceNumber);
        Assert.Equal(12.5, details.SettleAmount);
    }

    // Building a request from an Order needs Dynamicweb's services (currency rounding, line items), which
    // only run inside a host, so the card-on-file rule is tested on the request it is applied to.
    [Fact]
    public void A_saved_card_payment_is_sent_as_a_stored_credential_transaction()
    {
        _handler.Enqueue($$"""{"transactionResponse":{"responseCode":"1","transId":"1"},{{OkMessages}}}""");
        var request = new TransactionRequestType { Amount = 10, Order = new OrderType { InvoiceNumber = "CART42" } };
        var profile = new CustomerProfilePaymentType { CustomerProfileId = "10", PaymentProfile = new PaymentProfile { PaymentProfileId = "20" } };

        AuthorizeNetService.PrepareCustomerProfileCharge(request, profile);
        CreateService().CreateTransaction(request, "DW-42");

        JsonElement root = JsonDocument.Parse(Assert.Single(_handler.RequestBodies)).RootElement.GetProperty("createTransactionRequest");
        JsonElement transactionRequest = root.GetProperty("transactionRequest");
        Assert.Equal("DW-42", root.GetProperty("refId").GetString());
        Assert.True(transactionRequest.GetProperty("processingOptions").GetProperty("isStoredCredentials").GetBoolean());
        Assert.Equal("20", transactionRequest.GetProperty("profile").GetProperty("paymentProfile").GetProperty("paymentProfileId").GetString());
    }
}

/// <summary>
/// The stub client is a static override, so these tests never run in parallel with each other.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ServiceRoundTripCollection
{
    public const string Name = "Service round trips";
}
