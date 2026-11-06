using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

public class HelperTests
{
    [Theory]
    [InlineData("ORDER1042", "ORDER1042")]
    [InlineData("12345678901234567890", "12345678901234567890")]
    [InlineData("123456789012345678901", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void RefId_is_the_order_id_when_it_fits_the_limit(string? orderId, string? expected)
    {
        Assert.Equal(expected, OrderHelper.GetRefId(orderId));
    }

    [Theory]
    [InlineData(10.00, 10.00, true)]
    [InlineData(10.00, 10.009, true)]
    [InlineData(10.00, 10.02, false)]
    [InlineData(0, 0.5, false)]
    public void Amounts_are_compared_with_one_cent_tolerance(double left, double right, bool expected)
    {
        Assert.Equal(expected, OrderHelper.AmountsEqual(left, right));
    }

    [Theory]
    [InlineData("1", "Approved")]
    [InlineData("2", "Declined")]
    [InlineData("3", "Error")]
    [InlineData("4", "Held for Review")]
    [InlineData("9", "Unknown")]
    [InlineData("x", "Unknown")]
    [InlineData(null, "Unknown")]
    public void Response_codes_have_texts(string? code, string expected)
    {
        Assert.Equal(expected, TransactionDetailsHelper.GetResponseTextByCode(code));
    }

    [Fact]
    public void Transaction_status_texts_cover_unknown_and_null()
    {
        Assert.Equal("Unknown", TransactionDetailsHelper.GetTransactionStatus(TransactionStatusEnum.Unknown));
        Assert.Equal("Unknown", TransactionDetailsHelper.GetTransactionStatus(null));
        Assert.Equal("Voided", TransactionDetailsHelper.GetTransactionStatus(TransactionStatusEnum.Voided));
    }

    [Theory]
    [InlineData("abcdef", 3, "abc")]
    [InlineData("abc", 5, "abc")]
    [InlineData("", 5, "")]
    [InlineData(null, 5, "")]
    public void Crop_limits_length(string? value, int max, string expected)
    {
        Assert.Equal(expected, StringHelper.Crop(value, max));
    }

    [Fact]
    public void First_name_comes_from_the_first_populated_field()
    {
        string firstName = AuthorizeNetModelFactory.GetCustomerFirstNameOrFallback([null, "  ", "Jane Q Doe", "Other"]);

        Assert.Equal("Jane", firstName);
    }

    [Fact]
    public void Last_name_is_the_rest_of_a_full_name()
    {
        string lastName = AuthorizeNetModelFactory.GetCustomerLastNameOrFallback([null, "Jane Q Doe"]);

        Assert.Equal("Q Doe", lastName);
    }

    [Fact]
    public void Names_fall_back_when_nothing_is_set()
    {
        Assert.Equal("Customer", AuthorizeNetModelFactory.GetCustomerFirstNameOrFallback([null, ""]));
        Assert.Equal("Name", AuthorizeNetModelFactory.GetCustomerLastNameOrFallback(["Jane", ""]));
    }

    [Fact]
    public void Enum_member_values_are_used_for_settings()
    {
        Assert.Equal("hostedPaymentReturnOptions", SettingEnum.HostedPaymentReturnOptions.ToEnumMemberValue());
        Assert.Equal("hostedPaymentPaymentOptions", SettingEnum.HostedPaymentPaymentOptions.ToEnumMemberValue());
    }
}
