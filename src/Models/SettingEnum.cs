using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;

[JsonConverter(typeof(DataContractEnumConverter<SettingEnum>))]
internal enum SettingEnum
{
    [EnumMember(Value = "emailCustomer")]
    EmailCustomer, //true/false. Used by createTransaction method.

    [EnumMember(Value = "merchantEmail")]
    MerchantEmail, //string. Used by createTransaction method.

    [EnumMember(Value = "allowPartialAuth")]
    AllowPartialAuth, //true/false. Used by createTransaction method.

    [EnumMember(Value = "headerEmailReceipt")]
    HeaderEmailReceipt, //string. Used by createTransaction method.

    [EnumMember(Value = "footerEmailReceipt")]
    FooterEmailReceipt, //string. Used by createTransaction method.

    [EnumMember(Value = "recurringBilling")]
    RecurringBilling, //true/false. Used by createTransaction method.

    [EnumMember(Value = "duplicateWindow")]
    DuplicateWindow, //number. Used by createTransaction method.

    [EnumMember(Value = "testRequest")]
    TestRequest, //true/false. Used by createTransaction method.

    [EnumMember(Value = "hostedProfileReturnUrl")]
    HostedProfileReturnUrl, //string. Used by getHostedProfilePage method.

    [EnumMember(Value = "hostedProfileReturnUrlText")]
    HostedProfileReturnUrlText, //string. Used by getHostedProfilePage method.

    [EnumMember(Value = "hostedProfilePageBorderVisible")]
    HostedProfilePageBorderVisible, //true/false. Used by getHostedProfilePage method.

    [EnumMember(Value = "hostedProfileIFrameCommunicatorUrl")]
    HostedProfileIFrameCommunicatorUrl, //string. Used by getHostedProfilePage method.

    [EnumMember(Value = "hostedProfileHeadingBgColor")]
    HostedProfileHeadingBgColor, //#e0e0e0. Used by getHostedProfilePage method.

    [EnumMember(Value = "hostedProfileValidationMode")]
    HostedProfileValidationMode, // liveMode/testMode liveMode: generates a transaction to the processor in the amount of 0.01 or 0.00. is immediately voided, if successful. testMode: performs field validation only, all fields are validated except unrestricted field definitions (viz. telephone number) do not generate errors. If a validation transaction is unsuccessful, the profile is not created, and the merchant receives an error. 

    [EnumMember(Value = "hostedProfileBillingAddressRequired")]
    HostedProfileBillingAddressRequired, //true/false. If true, sets First Name, Last Name, Address, City, State, and Zip Code as required fields in order for a payment profile to be created or updated within a hosted CIM form.

    [EnumMember(Value = "hostedProfileCardCodeRequired")]
    HostedProfileCardCodeRequired, //true/false. If true, sets the Card Code field as required in order for a payment profile to be created or updated within a hosted CIM form.

    [EnumMember(Value = "hostedProfileBillingAddressOptions")]
    HostedProfileBillingAddressOptions, // showBillingAddress/showNone showBillingAddress: Allow merchant to show billing address. showNone : Hide billing address and billing name. 

    [EnumMember(Value = "hostedProfileManageOptions")]
    HostedProfileManageOptions, // showAll/showPayment/ShowShipping showAll: Shipping and Payment profiles are shown on the manage page, this is the default. showPayment : Only Payment profiles are shown on the manage page. showShipping : Only Shippiung profiles are shown on the manage page. 

    [EnumMember(Value = "hostedPaymentIFrameCommunicatorUrl")]
    HostedPaymentIFrameCommunicatorUrl, //JSON string. Used by getHostedPaymentPage method.

    [EnumMember(Value = "hostedPaymentButtonOptions")]
    HostedPaymentButtonOptions, //JSON string. Used by getHostedPaymentPage method.

    [EnumMember(Value = "hostedPaymentReturnOptions")]
    HostedPaymentReturnOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentOrderOptions")]
    HostedPaymentOrderOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentPaymentOptions")]
    HostedPaymentPaymentOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentBillingAddressOptions")]
    HostedPaymentBillingAddressOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentShippingAddressOptions")]
    HostedPaymentShippingAddressOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentSecurityOptions")]
    HostedPaymentSecurityOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentCustomerOptions")]
    HostedPaymentCustomerOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "hostedPaymentStyleOptions")]
    HostedPaymentStyleOptions, //JSON string. Used by getHostedPaymentPage method

    [EnumMember(Value = "typeEmailReceipt")]
    TypeEmailReceipt, //JSON string. Used by sendCustomerTransactionReceipt method

    [EnumMember(Value = "hostedProfilePaymentOptions")]
    HostedProfilePaymentOptions, // showAll/showCreditCard/showBankAccount showAll: both CreditCard and BankAccount sections will be shown on Add payment page, this is the default. showCreditCard :only CreditCard payment form will be shown on Add payment page. showBankAccount :only BankAccount payment form will be shown on Add payment page. 

    [EnumMember(Value = "hostedProfileSaveButtonText")]
    HostedProfileSaveButtonText, //string. Used by getHostedProfilePage method to accept button text configuration.

    [EnumMember(Value = "hostedPaymentVisaCheckoutOptions")]
    HostedPaymentVisaCheckoutOptions //string. Used by getHostedPaymentPage method to accept VisaCheckout configuration.
}