using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

public class SecurityHelperTests
{
    [Fact]
    public void Masks_card_number_in_json_keeping_last_four_digits()
    {
        string masked = SecurityHelper.MaskSensitiveDataInContent("""{"creditCard":{"cardNumber":"4111111111111111","expirationDate":"2030-12"}}""");

        Assert.DoesNotContain("4111111111111111", masked);
        Assert.Contains("\"cardNumber\":\"XXXX-XXXX-XXXX-1111\"", masked);
    }

    [Fact]
    public void Masks_card_code()
    {
        string masked = SecurityHelper.MaskSensitiveDataInContent("""{"cardCode":"123"}""");

        Assert.Contains("\"cardCode\":\"XXX\"", masked);
    }

    [Fact]
    public void Masks_transaction_key_in_request_body()
    {
        string masked = SecurityHelper.MaskSensitiveDataInContent("""{"merchantAuthentication":{"name":"login","transactionKey":"9fK2Lm8Qw3Rt5Yu7"}}""");

        Assert.DoesNotContain("9fK2Lm8Qw3Rt5Yu7", masked);
        Assert.Contains("\"transactionKey\":\"***HIDDEN***\"", masked);
        Assert.Contains("\"name\":\"login\"", masked);
    }

    [Fact]
    public void Masks_basic_authorization_header()
    {
        string headers = "Accept: application/json\r\nAuthorization: Basic bG9naW46dHJhbnNhY3Rpb25LZXk=\r\n";

        string masked = SecurityHelper.MaskSensitiveDataInContent(headers);

        Assert.DoesNotContain("bG9naW46dHJhbnNhY3Rpb25LZXk=", masked);
        Assert.Contains("Authorization: Basic ***HIDDEN***", masked);
        Assert.Contains("Accept: application/json", masked);
    }

    [Fact]
    public void Masks_bearer_authorization_header_case_insensitively()
    {
        string masked = SecurityHelper.MaskSensitiveDataInContent("authorization: bearer abc.def.ghi");

        Assert.Equal("authorization: bearer ***HIDDEN***", masked);
    }

    [Fact]
    public void Leaves_non_sensitive_content_untouched()
    {
        const string content = """{"transId":"60020981676","responseCode":"1","accountNumber":"XXXX1111"}""";

        Assert.Equal(content, SecurityHelper.MaskSensitiveDataInContent(content));
    }

    [Theory]
    [InlineData("4111111111111111", "XXXX-XXXX-XXXX-1111")]
    [InlineData("4111 1111 1111 1111", "XXXX-XXXX-XXXX-1111")]
    [InlineData("12345678", "XXXX5678")]
    [InlineData("123", "XXX")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Masks_card_numbers_of_any_length(string? input, string expected)
    {
        Assert.Equal(expected, SecurityHelper.MaskCreditCardNumber(input));
    }

    [Fact]
    public void Configuration_is_valid_when_all_credentials_are_set()
    {
        SecurityValidationResult result = SecurityHelper.ValidateSecurityConfiguration("login", "key", "signature");

        Assert.True(result.IsValid);
        Assert.Empty(result.SecurityIssues);
    }

    [Fact]
    public void Configuration_reports_every_missing_credential()
    {
        SecurityValidationResult result = SecurityHelper.ValidateSecurityConfiguration("", " ", null);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.SecurityIssues.Count);
        Assert.Contains(result.SecurityIssues, issue => issue.Contains("Signature Key"));
    }
}
