using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

/// <summary>
/// Typed access to the Authorize.Net operations the checkout handler uses.
/// API reference: https://developer.authorize.net/api/reference/index.html
/// </summary>
internal sealed class AuthorizeNetService
{
    private const int UnsettledTransactionPageSize = 1000;

    /// <summary>
    /// How many pages of unsettled transactions a lookup by invoice number scans (newest first).
    /// </summary>
    private const int UnsettledTransactionMaxPages = 5;

    private readonly AuthorizeNetHttpService _httpService;
    private readonly string _apiLoginId;
    private readonly string _transactionKey;
    private readonly AuthorizeNetLogger _logger;

    /// <param name="apiLoginId">API Login ID of the merchant account</param>
    /// <param name="transactionKey">Transaction Key of the merchant account</param>
    /// <param name="isTestMode">True for the sandbox environment</param>
    /// <param name="debugLogging">True to log every request and response</param>
    /// <param name="order">Optional order; log entries are then also saved as order debugging info</param>
    public AuthorizeNetService(string apiLoginId, string transactionKey, bool isTestMode, bool debugLogging, Order? order)
    {
        _apiLoginId = apiLoginId;
        _transactionKey = transactionKey;
        _logger = new AuthorizeNetLogger(debugLogging, order);
        _httpService = new AuthorizeNetHttpService(isTestMode, _logger);
    }

    #region Payments

    /// <summary>
    /// Requests an Accept Hosted form token (getHostedPaymentPageRequest).
    /// </summary>
    public GetHostedPaymentPageResponse? GetHostedPaymentPage(Order order, double orderAmount, TransactionType transactionType, bool saveCard, HostedPaymentSettings settings)
    {
        TransactionRequestType transactionRequest = CreateBasePaymentTransactionRequest(order, orderAmount, transactionType);
        AddCustomerData(transactionRequest, order);

        if (saveCard)
            transactionRequest.ProcessingOptions = new ProcessingOptionsType { IsStoredCredentials = true };

        var wrapper = new GetHostedPaymentPageRequestWrapper
        {
            GetHostedPaymentPageRequest = new GetHostedPaymentPageRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                RefId = OrderHelper.GetRefId(order),
                TransactionRequest = transactionRequest,
                HostedPaymentSettings = settings
            }
        };

        return _httpService.Post<GetHostedPaymentPageResponse>(AuthorizeNetJson.Serialize(wrapper));
    }

    /// <summary>
    /// Charges a stored payment profile (saved card) for the order amount.
    /// </summary>
    public CreateTransactionResponse? ChargeCustomerProfile(Order order, double orderAmount, TransactionType transactionType, CustomerProfilePaymentType profileToCharge)
    {
        TransactionRequestType request = CreateBasePaymentTransactionRequest(order, orderAmount, transactionType);
        request.Profile = profileToCharge;

        return CreateTransaction(request, OrderHelper.GetRefId(order));
    }

    /// <summary>
    /// Voids the order's transaction. Only possible before the batch settles (same day).
    /// </summary>
    public CreateTransactionResponse? Void(Order order)
    {
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.VoidTransaction,
            RefTransId = order.TransactionNumber,
            Order = new Models.OrderType { InvoiceNumber = order.Id }
        };

        return CreateTransaction(transactionRequest, OrderHelper.GetRefId(order));
    }

    /// <summary>
    /// Captures a previously authorized transaction (priorAuthCaptureTransaction).
    /// Authorize.Net allows one capture per authorization.
    /// </summary>
    public CreateTransactionResponse? Capture(Order order, double captureAmount)
    {
        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.PriorAuthCaptureTransaction,
            Amount = captureAmount,
            CurrencyCode = order.CurrencyCode,
            RefTransId = order.TransactionNumber,
            Order = new Models.OrderType { InvoiceNumber = order.Id }
        };

        return CreateTransaction(transactionRequest, OrderHelper.GetRefId(order));
    }

    /// <summary>
    /// Refunds part or all of a settled transaction (refundTransaction).
    /// Authorize.Net requires the last four digits of the card used for the original transaction.
    /// </summary>
    public CreateTransactionResponse? Refund(Order order, double refundAmount)
    {
        string cardNumber = order.TransactionCardNumber ?? "";
        if (cardNumber.Length < 4)
            throw new AuthorizeNetApiException("The last four digits of the card number are required for a refund.");

        var transactionRequest = new TransactionRequestType
        {
            TransactionType = TransactionTypeEnum.RefundTransaction,
            Amount = refundAmount,
            CurrencyCode = order.CurrencyCode,
            RefTransId = order.TransactionNumber,
            Order = new Models.OrderType { InvoiceNumber = order.Id },
            Payment = new PaymentType
            {
                CreditCard = new CreditCardType
                {
                    CardNumber = cardNumber[^4..],
                    ExpirationDate = SecuritySettings.CreditCardExpirationMask
                }
            }
        };

        return CreateTransaction(transactionRequest, OrderHelper.GetRefId(order));
    }

    /// <summary>
    /// Sends a createTransactionRequest.
    /// </summary>
    public CreateTransactionResponse? CreateTransaction(TransactionRequestType transactionRequest, string? refId)
    {
        var wrapper = new CreateTransactionRequestWrapper
        {
            CreateTransactionRequest = new CreateTransactionRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                RefId = refId,
                TransactionRequest = transactionRequest
            }
        };

        return _httpService.Post<CreateTransactionResponse>(AuthorizeNetJson.Serialize(wrapper));
    }

    private TransactionRequestType CreateBasePaymentTransactionRequest(Order order, double orderAmount, TransactionType transactionType) => new()
    {
        Amount = orderAmount,
        CurrencyCode = order.CurrencyCode,
        LineItems = AuthorizeNetModelFactory.CreateLineItems(order),
        Order = new Models.OrderType { InvoiceNumber = order.Id },
        CustomerIp = StringHelper.Crop(order.Ip, 15),
        TransactionType = transactionType is TransactionType.AuthCaptureTransaction
            ? TransactionTypeEnum.AuthCaptureTransaction
            : TransactionTypeEnum.AuthOnlyTransaction
    };

    private static void AddCustomerData(TransactionRequestType request, Order order)
    {
        request.BillTo = AuthorizeNetModelFactory.CreateBillAddress(order);
        request.Customer = new CustomerDataType
        {
            Id = order.CustomerAccessUserId > 0 ? order.CustomerAccessUserId.ToString() : null,
            Email = order.CustomerEmail
        };

        NameAndAddressType shipAddress = AuthorizeNetModelFactory.CreateShipAddress(order);
        if (!string.IsNullOrEmpty(shipAddress.Address))
            request.ShipTo = shipAddress;
    }

    #endregion

    #region Reporting

    /// <summary>
    /// Gets full details of a transaction (getTransactionDetailsRequest).
    /// </summary>
    /// <exception cref="AuthorizeNetApiException">When the API returns an error or no transaction</exception>
    public TransactionDetailsType GetTransactionDetails(string transactionId)
    {
        var wrapper = new GetTransactionDetailsRequestWrapper
        {
            GetTransactionDetailsRequest = new GetTransactionDetailsRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                TransId = transactionId,
            }
        };

        GetTransactionDetailsResponse? response = _httpService.Post<GetTransactionDetailsResponse>(AuthorizeNetJson.Serialize(wrapper));
        TransactionDetailsType? transactionDetails = response?.Transaction;

        if (transactionDetails is not null && !string.IsNullOrWhiteSpace(transactionDetails.TransId))
            return transactionDetails;

        throw new AuthorizeNetApiException($"Transaction {transactionId} was not returned by getTransactionDetailsRequest.");
    }

    /// <summary>
    /// Lists unsettled transactions, newest first (getUnsettledTransactionListRequest).
    /// </summary>
    public List<TransactionSummaryType> GetUnsettledTransactions(int page = 1, int limit = UnsettledTransactionPageSize)
    {
        var wrapper = new GetUnsettledTransactionListRequestWrapper
        {
            GetUnsettledTransactionListRequest = new GetUnsettledTransactionListRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                Sorting = new TransactionListSorting { OrderBy = "submitTimeUTC", OrderDescending = true },
                Paging = new TransactionListPaging { Limit = Math.Clamp(limit, 1, UnsettledTransactionPageSize), Offset = Math.Max(1, page) }
            }
        };

        GetUnsettledTransactionListResponse? response = _httpService.Post<GetUnsettledTransactionListResponse>(AuthorizeNetJson.Serialize(wrapper));

        return response?.Transactions ?? [];
    }

    /// <summary>
    /// Finds the newest unsettled transaction created for an order (matched on invoiceNumber), scanning
    /// newest-first pages until found or the list is exhausted. Used to verify a hosted-form payment when the
    /// customer returns, since the redirect itself carries no data.
    /// </summary>
    public TransactionSummaryType? FindUnsettledTransaction(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return null;

        for (int page = 1; page <= UnsettledTransactionMaxPages; page++)
        {
            List<TransactionSummaryType> transactions = GetUnsettledTransactions(page);

            TransactionSummaryType? match = transactions
                .FirstOrDefault(transaction => string.Equals(transaction.InvoiceNumber?.Trim(), invoiceNumber, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
                return match;

            if (transactions.Count < UnsettledTransactionPageSize)
                break;
        }

        return null;
    }

    #endregion

    #region Customer profiles

    /// <summary>
    /// Gets the customer profile for a Dynamicweb user (merchantCustomerId = user id).
    /// When <paramref name="tryCreate"/> is true and no profile exists, an empty one is created.
    /// </summary>
    public CustomerProfileMaskedType? GetCustomerProfile(int userId, bool tryCreate)
    {
        var wrapper = new GetCustomerProfileRequestWrapper
        {
            GetCustomerProfileRequest = new GetCustomerProfileRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                MerchantCustomerId = userId.ToString(),
            }
        };

        GetCustomerProfileResponse? response = null;

        try
        {
            response = _httpService.Post<GetCustomerProfileResponse>(AuthorizeNetJson.Serialize(wrapper));
        }
        catch (AuthorizeNetApiException ex)
        {
            // E00040 "The record cannot be found" is the normal answer for a user without a profile.
            _logger.LogInfo("Customer profile for user {0} was not returned ({1}).{2}", userId, ex.Message,
                tryCreate ? " A new profile will be created." : "");
        }

        if (response?.Profile is not null && IsOk(response.Messages))
            return response.Profile;

        if (!tryCreate)
            return null;

        var createWrapper = new CreateCustomerProfileRequestWrapper
        {
            CreateCustomerProfileRequest = new CreateCustomerProfileRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                Profile = new CustomerProfileType { MerchantCustomerId = userId.ToString() },
            }
        };

        CreateCustomerProfileResponse? createResponse = _httpService.Post<CreateCustomerProfileResponse>(AuthorizeNetJson.Serialize(createWrapper));

        if (createResponse is not null && IsOk(createResponse.Messages) && !string.IsNullOrEmpty(createResponse.CustomerProfileId))
            return new CustomerProfileMaskedType { CustomerProfileId = createResponse.CustomerProfileId };

        return null;
    }

    /// <summary>
    /// Stores the payment method of a successful transaction on a customer profile and returns the payment profile id.
    /// </summary>
    public string CreatePaymentProfileFromTransaction(string transactionId, string customerProfileId)
    {
        var wrapper = new CreateCustomerProfileFromTransactionRequestWrapper
        {
            CreateCustomerProfileFromTransactionRequest = new CreateCustomerProfileFromTransactionRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                TransId = transactionId,
                CustomerProfileId = customerProfileId,
            }
        };

        CreateCustomerProfileResponse? response = _httpService.Post<CreateCustomerProfileResponse>(AuthorizeNetJson.Serialize(wrapper));

        return response?.CustomerPaymentProfileIdList?.FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Deletes a stored payment profile (saved card).
    /// </summary>
    public DeleteCustomerPaymentProfileResponse? DeleteCustomerPaymentProfile(string customerProfileId, string paymentProfileId)
    {
        var wrapper = new DeleteCustomerPaymentProfileRequestWrapper
        {
            DeleteCustomerPaymentProfileRequest = new DeleteCustomerPaymentProfileRequest
            {
                MerchantAuthentication = GetMerchantAuthentication(),
                CustomerProfileId = customerProfileId,
                CustomerPaymentProfileId = paymentProfileId,
            }
        };

        return _httpService.Post<DeleteCustomerPaymentProfileResponse>(AuthorizeNetJson.Serialize(wrapper));
    }

    #endregion

    #region Webhooks

    /// <summary>
    /// Lists the webhooks of the merchant account.
    /// </summary>
    public List<WebhookResponse> GetWebhooks()
    {
        WebhookResponse[]? webhooks = _httpService.Get<WebhookResponse[]>(WebhookEndpoints.Base, GetBasicAuthHeaders());
        return webhooks?.ToList() ?? [];
    }

    public WebhookResponse CreateWebhook(WebhookRequest request)
    {
        _logger.LogInfo("[WEBHOOK] Creating webhook for URL: {0}", request.Url ?? "");

        WebhookResponse? response = _httpService.Post<WebhookResponse>(WebhookEndpoints.Base, AuthorizeNetJson.Serialize(request), GetBasicAuthHeaders());

        return response ?? throw new AuthorizeNetApiException("Failed to create webhook - no response received");
    }

    public void DeleteWebhook(string webhookId)
    {
        if (string.IsNullOrWhiteSpace(webhookId))
            throw new ArgumentException("Webhook id is required", nameof(webhookId));

        _logger.LogInfo("[WEBHOOK] Deleting webhook: {0}", webhookId);
        _httpService.Delete(WebhookEndpoints.GetSpecific(webhookId), GetBasicAuthHeaders());
    }

    /// <summary>
    /// Makes sure exactly one active webhook with all required event types exists for the URL.
    /// Returns the new webhook when one was created, otherwise null.
    /// </summary>
    public WebhookResponse? EnsureWebhooksRegistered(string webhookUrl, bool forceRegistration)
    {
        List<WebhookResponse> ourWebhooks = GetWebhooks()
            .Where(webhook => WebhookRegistrationPolicy.MatchesUrl(webhook, webhookUrl))
            .ToList();

        WebhookRegistrationResult decision = WebhookRegistrationPolicy.Decide(ourWebhooks, forceRegistration);
        _logger.LogInfo("[WEBHOOK] Registration check for {0}: RequiresRegistration={1}, Reason={2}", webhookUrl, decision.RequiresRegistration, decision.Reason);

        if (!decision.RequiresRegistration)
            return null;

        foreach (WebhookResponse webhook in ourWebhooks.Where(webhook => !string.IsNullOrWhiteSpace(webhook.WebhookId)))
            DeleteWebhook(webhook.WebhookId!);

        return CreateWebhook(new WebhookRequest
        {
            Name = WebhookRegistrationPolicy.WebhookName,
            Url = webhookUrl,
            EventTypes = WebhookRegistrationPolicy.RequiredEventTypes,
            Status = "active"
        });
    }

    private Dictionary<string, string> GetBasicAuthHeaders()
    {
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiLoginId}:{_transactionKey}"));
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Basic {credentials}",
        };
    }

    #endregion

    private MerchantAuthenticationType GetMerchantAuthentication() => new()
    {
        Name = _apiLoginId,
        TransactionKey = _transactionKey
    };

    internal static bool IsOk(MessagesType? messages)
        => DataContractEnumConverter<MessageTypeEnum>.TryParse(messages?.ResultCode, out MessageTypeEnum code) && code is MessageTypeEnum.Ok;
}
