using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.Orders;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Builds Authorize.Net request fragments from a Dynamicweb order.
/// Field lengths follow the Authorize.Net API reference.
/// </summary>
internal static class AuthorizeNetModelFactory
{
    private const string FallbackCustomerFirstName = "Customer";
    private const string FallbackCustomerLastName = "Name";

    /// <summary>
    /// Maximum length of lineItem.itemId and lineItem.name.
    /// </summary>
    internal const int LineItemIdMaxLength = 31;
    internal const int LineItemNameMaxLength = 31;
    internal const int LineItemDescriptionMaxLength = 255;

    public static NameAndAddressType CreateShipAddress(Order order)
    {
        string firstName = GetCustomerFirstNameOrFallback([
            order.DeliveryFirstName,
            order.CustomerFirstName,
            order.DeliveryName,
            order.CustomerName
        ]);

        string lastName = GetCustomerLastNameOrFallback([
            order.DeliverySurname,
            order.CustomerSurname,
            order.DeliveryName,
            order.CustomerName
        ]);

        return new()
        {
            FirstName = StringHelper.Crop(firstName, 50),
            LastName = StringHelper.Crop(lastName, 50),
            Company = StringHelper.Crop(FirstNonEmpty(order.DeliveryCompany, order.CustomerCompany), 50),
            Address = StringHelper.Crop(order.DeliveryAddress, 60),
            City = StringHelper.Crop(order.DeliveryCity, 40),
            State = StringHelper.Crop(order.DeliveryRegion, 40),
            Zip = StringHelper.Crop(order.DeliveryZip, 20),
            Country = StringHelper.Crop(order.DeliveryCountryCode, 60)
        };
    }

    public static CustomerAddressType CreateBillAddress(Order order)
    {
        string firstName = GetCustomerFirstNameOrFallback([
            order.CustomerFirstName,
            order.CustomerName,
        ]);

        string lastName = GetCustomerLastNameOrFallback([
            order.CustomerSurname,
            order.CustomerName
        ]);

        return new()
        {
            FirstName = StringHelper.Crop(firstName, 50),
            LastName = StringHelper.Crop(lastName, 50),
            Company = StringHelper.Crop(order.CustomerCompany, 50),
            Address = StringHelper.Crop(order.CustomerAddress, 60),
            City = StringHelper.Crop(order.CustomerCity, 40),
            State = StringHelper.Crop(order.CustomerRegion, 40),
            Zip = StringHelper.Crop(order.CustomerZip, 20),
            Country = StringHelper.Crop(order.CustomerCountryCode, 60),
            PhoneNumber = StringHelper.Crop(order.CustomerPhone, 25),
            Email = StringHelper.Crop(order.CustomerEmail, 255)
        };
    }

    /// <summary>
    /// Maps product and fixed order lines to Authorize.Net line items.
    /// Unit price is the per-unit price (OrderLine.Price is the line total, quantity * unit price).
    /// </summary>
    public static LineItems CreateLineItems(Order order)
    {
        var result = new List<LineItem>();

        foreach (OrderLine line in order.OrderLines)
        {
            if (line.OrderLineType is not (OrderLineType.Product or OrderLineType.Fixed))
                continue;

            string name = FirstNonEmpty(line.ProductName, line.Product?.Name, line.ProductNumber, line.Id);

            result.Add(new()
            {
                ItemId = StringHelper.Crop(FirstNonEmpty(line.ProductNumber, line.Id), LineItemIdMaxLength),
                Name = StringHelper.Crop(name, LineItemNameMaxLength),
                Description = StringHelper.Crop(line.Product?.ShortDescription, LineItemDescriptionMaxLength),
                Quantity = Converter.ToDouble(line.Quantity),
                UnitPrice = Ecommerce.Services.Currencies.Round(order.Currency, line.UnitPrice.Price),
            });
        }

        return new()
        {
            LineItem = result
        };
    }

    internal static string GetCustomerFirstNameOrFallback(IEnumerable<string?> fields)
    {
        string? firstName = fields
            .Select(field => field?.Trim().Split(' ').FirstOrDefault())
            .FirstOrDefault(field => !string.IsNullOrWhiteSpace(field));

        return firstName ?? FallbackCustomerFirstName;
    }

    internal static string GetCustomerLastNameOrFallback(IEnumerable<string?> fields)
    {
        string? lastName = fields
            .Select(field => string.Join(" ", field?.Trim().Split(' ').Skip(1) ?? []))
            .FirstOrDefault(field => !string.IsNullOrWhiteSpace(field));

        return lastName ?? FallbackCustomerLastName;
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
