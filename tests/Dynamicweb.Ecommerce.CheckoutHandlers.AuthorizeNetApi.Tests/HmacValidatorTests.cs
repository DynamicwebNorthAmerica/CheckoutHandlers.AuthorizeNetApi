using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

public class HmacValidatorTests
{
    private const string Key = "8A9C1F2D3E4B5A6978C0D1E2F3A4B5C6D7E8F9A0B1C2D3E4F5A6B7C8D9E0F1A2";
    private const string Body = """{"notificationId":"a1","eventType":"net.authorize.payment.authcapture.created","payload":{"id":"60020981676"}}""";

    [Fact]
    public void Accepts_signature_computed_with_the_same_key()
    {
        string signature = HmacValidator.ComputeSignatureHex(Key, Body);

        Assert.True(HmacValidator.IsValid(Key, Body, signature));
    }

    [Fact]
    public void Accepts_sha512_prefix_and_lower_case_hex()
    {
        string signature = "sha512=" + HmacValidator.ComputeSignatureHex(Key, Body).ToLowerInvariant();

        Assert.True(HmacValidator.IsValid(Key, Body, signature));
    }

    [Fact]
    public void Trims_whitespace_around_key_and_signature()
    {
        string signature = "  " + HmacValidator.ComputeSignatureHex(Key, Body) + "\n";

        Assert.True(HmacValidator.IsValid(" " + Key + " ", Body, signature));
    }

    [Fact]
    public void Rejects_tampered_body()
    {
        string signature = HmacValidator.ComputeSignatureHex(Key, Body);

        Assert.False(HmacValidator.IsValid(Key, Body.Replace("60020981676", "60020981677"), signature));
    }

    [Fact]
    public void Rejects_wrong_key()
    {
        string signature = HmacValidator.ComputeSignatureHex(Key, Body);

        Assert.False(HmacValidator.IsValid(Key + "0", Body, signature));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sha512=")]
    [InlineData("not-hex")]
    [InlineData("sha512=ZZZZ")]
    public void Rejects_missing_or_malformed_signature(string? signature)
    {
        Assert.False(HmacValidator.IsValid(Key, Body, signature));
    }

    [Fact]
    public void Rejects_missing_key_or_body()
    {
        string signature = HmacValidator.ComputeSignatureHex(Key, Body);

        Assert.False(HmacValidator.IsValid(null, Body, signature));
        Assert.False(HmacValidator.IsValid("", Body, signature));
        Assert.False(HmacValidator.IsValid(Key, null, signature));
        Assert.False(HmacValidator.IsValid(Key, "", signature));
    }
}
