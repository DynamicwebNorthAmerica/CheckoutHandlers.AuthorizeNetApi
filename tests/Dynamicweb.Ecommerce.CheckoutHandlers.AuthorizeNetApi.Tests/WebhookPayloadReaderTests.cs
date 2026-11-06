using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

/// <summary>
/// Parsing of real-shaped webhook notification bodies.
/// </summary>
public class WebhookPayloadReaderTests
{
    // Example from https://developer.authorize.net/api/reference/features/webhooks.html, with the fields this handler relies on.
    private const string AuthCaptureNotification = """
        {
          "notificationId": "d0e9ac4c-5ec1-4fb1-a5a0-5e8a4d7f4a11",
          "eventType": "net.authorize.payment.authcapture.created",
          "eventDate": "2026-10-28T10:15:30.0000000Z",
          "webhookId": "7be120d3-2247-4706-b9b1-98931fdfdcce",
          "payload": {
            "responseCode": 1,
            "merchantReferenceId": "ORDER1042",
            "authCode": "LZ6I19",
            "avsResponse": "Y",
            "authAmount": 45.00,
            "invoiceNumber": "ORDER1042",
            "entityName": "transaction",
            "id": "60020981676"
          }
        }
        """;

    [Fact]
    public void Parses_notification_and_payload()
    {
        // Deserialize directly first so a parsing exception surfaces in the test output.
        Assert.NotNull(AuthorizeNetJson.Deserialize<NotificationItem>(AuthCaptureNotification));

        NotificationItem? item = WebhookPayloadReader.Parse(AuthCaptureNotification);

        Assert.NotNull(item);
        Assert.Equal(NotificationEventType.AuthCaptureCreated, item.GetEventType());
        Assert.NotNull(item.Payload);
        Assert.Equal("60020981676", item.Payload.Id);
        Assert.Equal(1, item.Payload.ResponseCode);
        Assert.Equal(45.00, item.Payload.Amount);
    }

    [Fact]
    public void Prefers_merchantReferenceId_over_invoiceNumber()
    {
        NotificationItem? item = WebhookPayloadReader.Parse(AuthCaptureNotification.Replace("\"invoiceNumber\": \"ORDER1042\"", "\"invoiceNumber\": \"OTHER\""));

        Assert.True(WebhookPayloadReader.TryGetOrderId(item, out string? orderId));
        Assert.Equal("ORDER1042", orderId);
    }

    [Fact]
    public void Falls_back_to_invoiceNumber()
    {
        NotificationItem? item = WebhookPayloadReader.Parse(AuthCaptureNotification.Replace("\"merchantReferenceId\": \"ORDER1042\",", ""));

        Assert.True(WebhookPayloadReader.TryGetOrderId(item, out string? orderId));
        Assert.Equal("ORDER1042", orderId);
    }

    [Fact]
    public void Fails_without_any_order_reference()
    {
        NotificationItem? item = WebhookPayloadReader.Parse("""{"eventType":"net.authorize.payment.void.created","payload":{"id":"1","responseCode":1,"entityName":"transaction"}}""");

        Assert.NotNull(item);
        Assert.False(WebhookPayloadReader.TryGetOrderId(item, out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{"eventType":"net.authorize.payment.void.created"}""")]
    public void Returns_null_for_invalid_bodies(string? body)
    {
        Assert.Null(WebhookPayloadReader.Parse(body));
    }

    [Theory]
    [InlineData("net.authorize.payment.authorization.created", "AuthCreated")]
    [InlineData("net.authorize.payment.authcapture.created", "AuthCaptureCreated")]
    [InlineData("net.authorize.payment.capture.created", "CaptureCreated")]
    [InlineData("net.authorize.payment.priorAuthCapture.created", "PriorAuthCaptureCreated")]
    [InlineData("net.authorize.payment.refund.created", "RefundCreated")]
    [InlineData("net.authorize.payment.void.created", "VoidCreated")]
    [InlineData("net.authorize.payment.fraud.held", "FraudHeld")]
    [InlineData("net.authorize.payment.fraud.approved", "FraudApproved")]
    [InlineData("net.authorize.payment.fraud.declined", "FraudDeclined")]
    [InlineData("net.authorize.customer.created", null)]
    public void Maps_event_types(string eventType, string? expectedName)
    {
        Assert.Equal(expectedName, new NotificationItem { EventType = eventType }.GetEventType()?.ToString());
    }
}
