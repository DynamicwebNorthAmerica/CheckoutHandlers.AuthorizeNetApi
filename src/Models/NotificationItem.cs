using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

internal sealed class NotificationItem
{
    [JsonPropertyName("notificationId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Id { get; set; }

    [JsonPropertyName("eventType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EventType { get; set; }

    [JsonPropertyName("eventDate"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EventDate { get; set; }

    [JsonPropertyName("webhookId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? WebhookId { get; set; }

    [JsonPropertyName("payload"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public NotificationPayload? Payload { get; set; }

    public NotificationEventType? GetEventType() => EventType switch
    {
        "net.authorize.payment.authcapture.created" => NotificationEventType.AuthCaptureCreated,
        "net.authorize.payment.authorization.created" => NotificationEventType.AuthCreated,
        "net.authorize.payment.capture.created" => NotificationEventType.CaptureCreated,
        "net.authorize.payment.priorAuthCapture.created" => NotificationEventType.PriorAuthCaptureCreated,
        "net.authorize.payment.refund.created" => NotificationEventType.RefundCreated,
        "net.authorize.payment.void.created" => NotificationEventType.VoidCreated,
        "net.authorize.payment.fraud.approved" => NotificationEventType.FraudApproved,
        "net.authorize.payment.fraud.declined" => NotificationEventType.FraudDeclined,
        "net.authorize.payment.fraud.held" => NotificationEventType.FraudHeld,
        _ => null
    };
}
