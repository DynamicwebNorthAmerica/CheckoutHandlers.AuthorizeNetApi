using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System.Text.Json;
using Xunit;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Tests;

public class DataContractEnumConverterTests
{
    private sealed class Holder
    {
        public TransactionStatusEnum Status { get; set; }
        public TransactionStatusEnum? NullableStatus { get; set; }
        public TransactionTypeEnum Type { get; set; }
    }

    [Fact]
    public void Reads_enum_member_values()
    {
        Holder? holder = JsonSerializer.Deserialize<Holder>("""{"Status":"capturedPendingSettlement","NullableStatus":"FDSPendingReview","Type":"priorAuthCaptureTransaction"}""");

        Assert.NotNull(holder);
        Assert.Equal(TransactionStatusEnum.CapturedPendingSettlement, holder.Status);
        Assert.Equal(TransactionStatusEnum.FDSPendingReview, holder.NullableStatus);
        Assert.Equal(TransactionTypeEnum.PriorAuthCaptureTransaction, holder.Type);
    }

    [Fact]
    public void Reads_member_names_case_insensitively_as_fallback()
    {
        Holder? holder = JsonSerializer.Deserialize<Holder>("""{"Status":"VOIDED"}""");

        Assert.Equal(TransactionStatusEnum.Voided, holder!.Status);
    }

    [Fact]
    public void Unknown_values_map_to_Unknown_instead_of_the_first_member()
    {
        Holder? holder = JsonSerializer.Deserialize<Holder>("""{"Status":"someNewStatusFromAuthorizeNet","Type":"someNewType"}""");

        Assert.Equal(TransactionStatusEnum.Unknown, holder!.Status);
        Assert.Equal(TransactionTypeEnum.Unknown, holder.Type);
    }

    [Fact]
    public void Null_keeps_nullable_enum_null()
    {
        Holder? holder = JsonSerializer.Deserialize<Holder>("""{"NullableStatus":null}""");

        Assert.Null(holder!.NullableStatus);
    }

    [Fact]
    public void Writes_enum_member_values()
    {
        string json = JsonSerializer.Serialize(new Holder
        {
            Status = TransactionStatusEnum.AuthorizedPendingCapture,
            Type = TransactionTypeEnum.AuthCaptureTransaction
        });

        Assert.Contains("\"Status\":\"authorizedPendingCapture\"", json);
        Assert.Contains("\"Type\":\"authCaptureTransaction\"", json);
    }

    [Theory]
    [InlineData("Ok", true, "Ok")]
    [InlineData("ok", true, "Ok")]
    [InlineData("Error", true, "Error")]
    [InlineData("Nope", false, null)]
    [InlineData(null, false, null)]
    public void TryParse_handles_result_codes(string? value, bool expectedParsed, string? expectedName)
    {
        bool parsed = DataContractEnumConverter<MessageTypeEnum>.TryParse(value, out MessageTypeEnum result);

        Assert.Equal(expectedParsed, parsed);
        if (parsed)
            Assert.Equal(expectedName, result.ToString());
    }
}
