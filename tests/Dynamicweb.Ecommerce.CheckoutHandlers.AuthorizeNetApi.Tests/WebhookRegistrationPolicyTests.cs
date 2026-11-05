using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System.Linq;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

public class WebhookRegistrationPolicyTests
{
    private const string Url = "https://shop.example.com/dwapi/ecommerce/carts/callback/Authorize.Net%20API";

    private static WebhookResponse Webhook(string[]? events = null, string status = "active", string url = Url) => new()
    {
        WebhookId = "wh-1",
        Url = url,
        Status = status,
        EventTypes = events ?? WebhookRegistrationPolicy.RequiredEventTypes
    };

    [Fact]
    public void Registers_when_no_webhook_exists()
    {
        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([], forceRegistration: false);

        Assert.True(result.RequiresRegistration);
    }

    [Fact]
    public void Registers_when_forced_even_if_configured()
    {
        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook()], forceRegistration: true);

        Assert.True(result.RequiresRegistration);
        Assert.Contains("Force", result.Reason);
    }

    [Fact]
    public void Does_nothing_when_one_active_webhook_has_all_events()
    {
        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook()], forceRegistration: false);

        Assert.False(result.RequiresRegistration);
    }

    [Fact]
    public void Event_type_comparison_ignores_case()
    {
        string[] upper = WebhookRegistrationPolicy.RequiredEventTypes.Select(e => e.ToUpperInvariant()).ToArray();

        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook(upper)], forceRegistration: false);

        Assert.False(result.RequiresRegistration);
    }

    [Fact]
    public void Registers_when_events_are_missing()
    {
        string[] partial = WebhookRegistrationPolicy.RequiredEventTypes.Take(2).ToArray();

        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook(partial)], forceRegistration: false);

        Assert.True(result.RequiresRegistration);
        Assert.Contains("net.authorize.payment.refund.created", result.Reason);
    }

    [Fact]
    public void Registers_when_webhook_is_inactive()
    {
        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook(status: "inactive")], forceRegistration: false);

        Assert.True(result.RequiresRegistration);
    }

    [Fact]
    public void Consolidates_duplicates()
    {
        WebhookRegistrationResult result = WebhookRegistrationPolicy.Decide([Webhook(), Webhook()], forceRegistration: false);

        Assert.True(result.RequiresRegistration);
        Assert.Contains("Multiple", result.Reason);
    }

    [Theory]
    [InlineData(Url, true)]
    [InlineData(Url + "/", true)]
    [InlineData("HTTPS://SHOP.EXAMPLE.COM/dwapi/ecommerce/carts/callback/Authorize.Net%20API", true)]
    [InlineData("https://other.example.com/dwapi/ecommerce/carts/callback/Authorize.Net%20API", false)]
    [InlineData(null, false)]
    public void Matches_urls_ignoring_case_and_trailing_slash(string? registeredUrl, bool expected)
    {
        Assert.Equal(expected, WebhookRegistrationPolicy.MatchesUrl(Webhook(url: registeredUrl!), Url));
    }

    [Fact]
    public void Required_events_cover_every_event_the_handler_processes()
    {
        var item = new NotificationItem();
        foreach (string eventType in WebhookRegistrationPolicy.RequiredEventTypes)
        {
            item.EventType = eventType;
            Assert.NotNull(item.GetEventType());
        }
    }
}
