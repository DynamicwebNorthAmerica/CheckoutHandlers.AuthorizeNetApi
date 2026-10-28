using Dynamicweb.Core;
using Dynamicweb.Ecommerce.Cart;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.Gateways;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.Frontend;
using Dynamicweb.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static Dynamicweb.Ecommerce.Orders.OrderCaptureInfo;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi;

/// <summary>
/// Authorize.Net checkout handler built on Accept Hosted (the Authorize.Net hosted payment page),
/// customer profiles for saved cards and webhooks for capture, refund and void synchronization.
/// API reference: https://developer.authorize.net/api/reference/index.html
/// Webhooks: https://developer.authorize.net/api/reference/features/webhooks.html
/// </summary>
[AddInName("Authorize.Net API"), AddInDescription("AuthorizeNet API Checkout Handler"), AddInUseParameterGrouping(true)]
public class AuthorizeNetCheckoutHandler : CheckoutHandler, ICancelOrder, IFullReturn, IPartialReturn, IRemoteCapture, ISavedCardAction, IParameterOptions, ICheckoutHandlerCallback
{
    private const string ReceiptAction = "Receipt";
    private const string CancelAction = "Cancel";
    private const string SignatureHeaderName = "X-ANET-Signature";
    private const string RefundTransactionMarkerFormat = "[RefundTransId:{0}]";
    private const int HostedFormVerificationAttempts = 3;
    private static readonly TimeSpan HostedFormVerificationDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan WebhookCheckInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// Remembers when the webhook subscription was last verified per environment, account and URL,
    /// so auto-registration does not call the Webhooks API on every checkout.
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> WebhookChecks = new();

    private TransactionType _transactionType = TransactionType.AuthCaptureTransaction;
    private string _cancelTemplate = "";
    private string _errorTemplate = "";

    #region AddIn parameters

    [AddInParameter("API login ID"), AddInParameterEditor(typeof(TextParameterEditor), "")]
    public string ApiLoginId { get; set; } = "";

    [AddInParameter("Transaction key"), AddInParameterEditor(typeof(TextParameterEditor), "")]
    public string TransactionKey { get; set; } = "";

    [AddInParameter("Signature key"), AddInParameterEditor(typeof(TextParameterEditor), "TextArea=true")]
    public string SignatureKey { get; set; } = "";

    [AddInParameter("Allow save cards"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool AllowSaveCards { get; set; }

    [AddInLabel("The type of credit card transaction"), AddInParameter("TypeOfTransaction"), AddInParameterEditor(typeof(RadioParameterEditor), "SortBy=Key")]
    public string TypeOfTransaction
    {
        get => _transactionType.ToString();
        set
        {
            if (Enum.TryParse(value, out TransactionType parsed))
                _transactionType = parsed;
        }
    }

    [AddInLabel("Test mode"), AddInParameter("TestMode"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool TestMode { get; set; }

    [AddInLabel("Cancel template"), AddInParameter("CancelTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{TemplateFolders.CancelTemplateFolder}")]
    public string CancelTemplate
    {
        get => TemplateHelper.GetTemplateName(_cancelTemplate);
        set => _cancelTemplate = value;
    }

    [AddInLabel("Error template"), AddInParameter("ErrorTemplate"), AddInParameterGroup("Template settings"), AddInParameterEditor(typeof(TemplateParameterEditor), $"folder=Templates/{TemplateFolders.ErrorTemplateFolder}")]
    public string ErrorTemplate
    {
        get => TemplateHelper.GetTemplateName(_errorTemplate);
        set => _errorTemplate = value;
    }

    [AddInLabel("Enable debug logging"), AddInParameter("DebugLogging"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool DebugLogging { get; set; }

    [AddInLabel("Enable auto webhook registration"), AddInParameter("EnableAutoWebhookRegistration"), AddInParameterGroup("Webhook settings"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool EnableAutoWebhookRegistration { get; set; }

    [AddInLabel("Force webhook re-registration"), AddInParameter("ForceWebhookRegistration"), AddInParameterGroup("Webhook settings"), AddInParameterEditor(typeof(YesNoParameterEditor), "")]
    public bool ForceWebhookRegistration { get; set; }

    #endregion

    public IEnumerable<ParameterOption> GetParameterOptions(string parameterName)
    {
        return parameterName switch
        {
            "TypeOfTransaction" =>
            [
                new("Authorization and Capture", nameof(TransactionType.AuthCaptureTransaction)),
                new("Authorization only", nameof(TransactionType.AuthOnlyTransaction))
            ],
            _ => []
        };
    }

    #region Checkout

    public override OutputResult BeginCheckout(Order order, CheckoutParameters? parameters)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            LogEvent(order, "Checkout started");

            SecurityValidationResult configuration = SecurityHelper.ValidateSecurityConfiguration(ApiLoginId, TransactionKey, SignatureKey);
            if (!configuration.IsValid)
                return OnError(order, "The payment method is not configured correctly.", $"Configuration errors: {string.Join("; ", configuration.SecurityIssues)}");

            TryRegisterWebhooks(order);

            return RedirectToHostedForm(order);
        }
        catch (Exception ex)
        {
            return OnError(order, "The payment could not be started.", $"Unhandled exception in BeginCheckout: {ex.Message}", ex);
        }
    }

    public override OutputResult HandleRequest(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            LogEvent(order, "Redirected to Authorize.Net checkout handler");

            string action = Converter.ToString(Context.Current?.Request?["action"]);

            if (string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(order.GatewayResult))
            {
                LogEvent(order, "GatewayResult found, but action is empty.");
                return ContentOutputResult.Empty;
            }

            return action switch
            {
                ReceiptAction => HandleHostedFormReturn(order),
                CancelAction => OrderCancelled(order),
                _ => ContentOutputResult.Empty
            };
        }
        catch (Exception ex)
        {
            return OnError(order, "The payment could not be completed.", $"Unhandled exception in HandleRequest: {ex.Message}", ex);
        }
    }

    private OutputResult RedirectToHostedForm(Order order)
    {
        LogEvent(order, "Requesting Accept Hosted form token");

        AuthorizeNetService service = GetAuthorizeNetService(order);
        double orderAmount = OrderHelper.GetOrderAmount(order);

        // Authorize.Net stores the return URLs inside a JSON setting; ampersands must be sent encoded.
        string encodedBaseUrl = GetBaseUrl(order).Replace("&", "%26");
        string receiptUrl = $"{encodedBaseUrl}%26action={ReceiptAction}";
        string cancelUrl = $"{encodedBaseUrl}%26action={CancelAction}";

        var settings = new HostedPaymentSettings
        {
            Setting =
            [
                new()
                {
                    SettingName = SettingEnum.HostedPaymentReturnOptions.ToEnumMemberValue(),
                    SettingValue = AuthorizeNetJson.Serialize(new HostedPaymentReturnOptions
                    {
                        ShowReceipt = false,
                        Url = receiptUrl,
                        CancelUrl = cancelUrl
                    })
                },
                new()
                {
                    SettingName = SettingEnum.HostedPaymentPaymentOptions.ToEnumMemberValue(),
                    SettingValue = AuthorizeNetJson.Serialize(new HostedPaymentPaymentOptions
                    {
                        ShowBankAccount = false
                    })
                },
            ]
        };

        GetHostedPaymentPageResponse? response = service.GetHostedPaymentPage(order, orderAmount, _transactionType, NeedSaveCard(order, out _), settings);

        if (IsResponseSuccessful(response, order))
        {
            return GetSubmitFormResult(AuthorizeNetEndpoints.GetHostedFormUrl(TestMode), new Dictionary<string, string>
            {
                ["token"] = response.Token
            });
        }

        Message? message = response?.Messages?.Message?.FirstOrDefault();
        return OnError(order, "The payment page could not be opened.", $"getHostedPaymentPageRequest failed ({message?.Code}): {message?.Text}");
    }

    /// <summary>
    /// The customer is back from the hosted payment page. The redirect carries no transaction data, so the
    /// payment is verified against the API before the order is completed: the newest unsettled transaction
    /// for this order id must exist, be approved or held for review, and match the order amount.
    /// </summary>
    private OutputResult HandleHostedFormReturn(Order order)
    {
        LogEvent(order, "Customer returned from the hosted payment page");

        if (order.Complete)
        {
            LogEvent(order, "Order is already complete (webhook processed before the customer returned)");
            return PassToCart(order);
        }

        AuthorizeNetService service = GetAuthorizeNetService(order);
        TransactionDetailsType? transaction = FindHostedFormTransaction(order, service);

        if (transaction is null)
        {
            return OnError(order,
                "The payment could not be verified. If your card was charged, please contact the store before trying again.",
                $"No unsettled Authorize.Net transaction found for order {order.Id} after {HostedFormVerificationAttempts} attempts.");
        }

        PaymentOutcome outcome = PaymentOutcome.FromTransactionDetails(transaction);

        if (outcome.ResponseCode is ResponseCodes.Declined or ResponseCodes.Error)
            return OrderRefused(order, transaction.ResponseReasonDescription ?? TransactionDetailsHelper.GetResponseTextByCode(outcome.ResponseCode));

        double orderAmount = OrderHelper.GetOrderAmount(order);
        if (!OrderHelper.AmountsEqual(outcome.Amount, orderAmount))
        {
            return OnError(order,
                "The payment amount does not match the order. Please contact the store.",
                $"Transaction {outcome.TransId} amount {outcome.Amount} does not match order amount {orderAmount}.");
        }

        return CompleteOrder(order, outcome);
    }

    private TransactionDetailsType? FindHostedFormTransaction(Order order, AuthorizeNetService service)
    {
        for (int attempt = 1; attempt <= HostedFormVerificationAttempts; attempt++)
        {
            TransactionSummaryType? summary = service.FindUnsettledTransaction(order.Id);
            if (summary is not null)
            {
                LogEvent(order, "Found transaction {0} for the order (attempt {1})", summary.TransId, attempt);
                return service.GetTransactionDetails(summary.TransId);
            }

            if (attempt < HostedFormVerificationAttempts)
                Thread.Sleep(HostedFormVerificationDelay);
        }

        return null;
    }

    /// <summary>
    /// Records an accepted payment on the order and completes the checkout.
    /// </summary>
    private OutputResult CompleteOrder(Order order, PaymentOutcome outcome)
    {
        LogEvent(order, "Payment accepted. Transaction: {0}, response code: {1}", outcome.TransId, outcome.ResponseCode);
        bool needSaveCard = NeedSaveCard(order, out string cardName);

        order.TransactionAmount = outcome.Amount;
        OrderHelper.UpdateTransactionNumber(order, outcome.TransId);
        order.TransactionStatus = outcome.ResponseCode == ResponseCodes.HeldForReview
            ? TransactionDetailsHelper.GetResponseTextByCode(ResponseCodes.HeldForReview)
            : outcome.TransactionStatus is { } status && status is not TransactionStatusEnum.Unknown
                ? TransactionDetailsHelper.GetTransactionStatus(status)
                : TransactionDetailsHelper.GetResponseTextByCode(outcome.ResponseCode);

        if (!string.IsNullOrEmpty(outcome.AccountNumber))
        {
            order.TransactionCardType = outcome.AccountType;
            order.TransactionCardNumber = outcome.AccountNumber;
        }

        if (outcome.IsCaptured)
        {
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Success, PreparedMessages.CaptureSuccessMessage);
            order.CaptureAmount = outcome.Amount;
        }

        if (needSaveCard)
        {
            if (!string.IsNullOrEmpty(order.TransactionNumber))
                SaveCard(order, cardName);
            else
                order.GatewayPaymentStatus = $"{SecuritySettings.SavedCardNamePlaceholder}{cardName}";
        }

        if (!order.Complete)
        {
            MarkOrderComplete(order, order.TransactionNumber);
            CheckoutDone(order);
        }
        else
        {
            Save(order);
        }

        return PassToCart(order);
    }

    private OutputResult OrderCancelled(Order order)
    {
        if (order.Complete)
        {
            LogEvent(order, "Cancel ignored because the order is already complete");
            return PassToCart(order);
        }

        LogEvent(order, "Order cancelled by the customer");
        order.TransactionStatus = "Cancelled";
        Save(order);
        CheckoutDone(order);

        var cancelTemplate = new Template(TemplateHelper.GetTemplatePath(CancelTemplate, TemplateFolders.CancelTemplateFolder));
        cancelTemplate.SetTag("CheckoutHandler:CancelMessage", "Payment has been cancelled before processing was completed");

        return new ContentOutputResult
        {
            Content = Render(order, cancelTemplate)
        };
    }

    private OutputResult OrderRefused(Order order, string? refusalReason)
        => OnError(order, $"Payment was refused. Reason: {refusalReason}", $"Payment refused by Authorize.Net: {refusalReason}");

    /// <summary>
    /// Logs the error, downgrades the order to a cart and renders the error template (or returns to the cart).
    /// The customer sees <paramref name="customerMessage"/>; details go to the log only.
    /// An already completed order is never downgraded.
    /// </summary>
    private OutputResult OnError(Order order, string customerMessage, string? logMessage = null, Exception? exception = null)
    {
        string detail = logMessage ?? customerMessage;
        if (exception is not null)
            LogError(order, exception, "{0}", detail);
        else
            LogError(order, "{0}", detail);

        if (order.Complete)
        {
            LogEvent(order, "Error ignored because the order is already complete: {0}", detail);
            return PassToCart(order);
        }

        order.TransactionAmount = 0;
        order.TransactionStatus = "Failed";
        order.Errors.Add(customerMessage);
        Save(order);

        Ecommerce.Services.Orders.DowngradeToCart(order);
        order.TransactionStatus = string.Empty;
        Common.Context.SetCart(order);

        if (string.IsNullOrWhiteSpace(ErrorTemplate))
            return PassToCart(order);

        var errorTemplate = new Template(TemplateHelper.GetTemplatePath(ErrorTemplate, TemplateFolders.ErrorTemplateFolder));
        errorTemplate.SetTag("CheckoutHandler:ErrorMessage", customerMessage);

        return new ContentOutputResult
        {
            Content = Render(order, errorTemplate)
        };
    }

    #endregion

    #region Saved cards (ISavedCardAction)

    public bool IsSavedCardSupported(Order order) => AllowSaveCards;

    /// <summary>
    /// Pays for the order with one of the customer's stored payment profiles.
    /// </summary>
    public OutputResult AuthorizeWithSavedCard(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            return AuthorizeWithSavedCardInternal(order);
        }
        catch (Exception ex)
        {
            return OnError(order, "The saved card could not be charged.", $"Saved card {order.SavedCardId} failed: {ex.Message}", ex);
        }
    }

    private OutputResult AuthorizeWithSavedCardInternal(Order order)
    {
        LogEvent(order, "Paying with saved card {0}", order.SavedCardId);

        if (order.SavedCardId <= 0)
            throw new ArgumentException($"Invalid saved card id: {order.SavedCardId}");

        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(order.SavedCardId);
        if (savedCard is null)
            throw new ArgumentException($"Saved card not found: {order.SavedCardId}");

        if (order.CustomerAccessUserId <= 0 || order.CustomerAccessUserId != savedCard.UserID)
            throw new UnauthorizedAccessException($"Saved card {order.SavedCardId} does not belong to user {order.CustomerAccessUserId}");

        if (string.IsNullOrEmpty(savedCard.Token))
            throw new ArgumentException($"Saved card {order.SavedCardId} has no payment profile token");

        AuthorizeNetService service = GetAuthorizeNetService(order);
        CustomerProfileMaskedType? profile = service.GetCustomerProfile(order.CustomerAccessUserId, tryCreate: false);
        if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            throw new InvalidOperationException($"Authorize.Net customer profile not found for user {order.CustomerAccessUserId}");

        var profileToCharge = new CustomerProfilePaymentType
        {
            CustomerProfileId = profile.CustomerProfileId,
            PaymentProfile = new PaymentProfile { PaymentProfileId = savedCard.Token }
        };

        double orderAmount = OrderHelper.GetOrderAmount(order);
        CreateTransactionResponse? response = service.ChargeCustomerProfile(order, orderAmount, _transactionType, profileToCharge);

        if (response?.TransactionResponse is not null
            && Converter.ToInt32(response.TransactionResponse.ResponseCode) == ResponseCodes.HeldForReview)
        {
            LogEvent(order, "Saved card transaction {0} is held for review", response.TransactionResponse.TransId);
            return CompleteOrder(order, PaymentOutcome.FromTransactionResponse(response.TransactionResponse, orderAmount, _transactionType));
        }

        if (!IsResponseSuccessful(response, order))
            return OrderRefused(order, GetErrorText(response));

        return CompleteOrder(order, PaymentOutcome.FromTransactionResponse(response.TransactionResponse, orderAmount, _transactionType));
    }

    /// <summary>
    /// Deletes a stored payment profile from the customer's Authorize.Net profile.
    /// </summary>
    public void DeleteSavedCard(int savedCardId)
    {
        LogSavedCardInfo($"Deleting saved card {savedCardId}");

        PaymentCardToken? savedCard = Ecommerce.Services.PaymentCard.GetById(savedCardId);
        if (savedCard is null)
        {
            LogSavedCardError($"Saved card {savedCardId} not found");
            return;
        }

        if (savedCard.UserID <= 0 || string.IsNullOrEmpty(savedCard.Token))
        {
            LogSavedCardError($"Saved card {savedCardId} has no user or token");
            return;
        }

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(null);

            CustomerProfileMaskedType? profile = service.GetCustomerProfile(savedCard.UserID, tryCreate: false);
            if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            {
                LogSavedCardError($"Customer profile not found for user {savedCard.UserID}");
                return;
            }

            DeleteCustomerPaymentProfileResponse? response = service.DeleteCustomerPaymentProfile(profile.CustomerProfileId, savedCard.Token);
            if (AuthorizeNetService.IsOk(response?.Messages))
                LogSavedCardInfo($"Deleted payment profile {savedCard.Token} from customer profile {profile.CustomerProfileId}");
            else
                LogSavedCardError($"Failed to delete payment profile: {response?.Messages?.Message?.FirstOrDefault()?.Text}");
        }
        catch (Exception ex)
        {
            LogSavedCardError($"Exception while deleting saved card {savedCardId}: {ex.Message}");
        }
    }

    private void LogSavedCardInfo(string message)
        => new AuthorizeNetLogger(DebugLogging).LogInfo("Saved card: {0}", message);

    private void LogSavedCardError(string message)
        => new AuthorizeNetLogger(DebugLogging).LogError("Saved card: {0}", message);

    private bool NeedSaveCard(Order order, out string cardName)
    {
        cardName = string.Empty;
        if (AllowSaveCards && order.CustomerAccessUserId > 0 && (order.DoSaveCardToken || !string.IsNullOrEmpty(order.SavedCardDraftName)))
        {
            cardName = !string.IsNullOrEmpty(order.SavedCardDraftName) ? order.SavedCardDraftName : order.Id;
            return true;
        }

        return false;
    }

    private void SaveCard(Order order, string cardName)
    {
        if (!AllowSaveCards || order.CustomerAccessUserId <= 0 || string.IsNullOrEmpty(order.TransactionNumber))
            return;

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            CustomerProfileMaskedType? profile = service.GetCustomerProfile(order.CustomerAccessUserId, tryCreate: true);
            if (string.IsNullOrEmpty(profile?.CustomerProfileId))
            {
                LogError(order, "Saved card: customer profile could not be found or created for user {0}", order.CustomerAccessUserId);
                return;
            }

            string cardToken = service.CreatePaymentProfileFromTransaction(order.TransactionNumber, profile.CustomerProfileId);
            if (string.IsNullOrEmpty(cardToken))
            {
                LogError(order, "Saved card: Authorize.Net did not return a payment profile for transaction {0}", order.TransactionNumber);
                return;
            }

            PaymentCardToken? paymentCard = Ecommerce.Services.PaymentCard.CreatePaymentCard(
                order.CustomerAccessUserId, order.PaymentMethodId ?? string.Empty, cardName,
                order.TransactionCardType ?? string.Empty, order.TransactionCardNumber ?? string.Empty, cardToken);

            if (paymentCard is null)
            {
                LogError(order, "Saved card: Dynamicweb did not create a payment card record");
                return;
            }

            order.SavedCardId = paymentCard.ID;
            Save(order);
            LogEvent(order, "Saved card: {0}", paymentCard.Name);
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Saved card could not be stored: {0}", ex.Message);
        }
    }

    #endregion

    #region Capture, cancel, return

    /// <summary>
    /// Captures the full order amount of an authorization. Authorize.Net allows one capture per authorization.
    /// </summary>
    public OrderCaptureInfo Capture(Order order)
    {
        if (order is null)
            return CaptureFailed(null, PreparedMessages.OrderNotSetMessage);

        LogEvent(order, "Capture requested");

        string errorText = OrderHelper.GetOrderError(order);
        if (!string.IsNullOrEmpty(errorText))
            return CaptureFailed(order, errorText);

        try
        {
            double captureAmount = OrderHelper.GetOrderAmount(order);
            AuthorizeNetService service = GetAuthorizeNetService(order);
            CreateTransactionResponse? response = service.Capture(order, captureAmount);

            if (!IsResponseSuccessful(response, order) || string.IsNullOrWhiteSpace(response.TransactionResponse?.TransId))
                return CaptureFailed(order, GetErrorText(response) ?? "Capture failed");

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(TransactionStatusEnum.CapturedPendingSettlement);
            Save(order);

            LogEvent(order, "Captured {0}", FormatAmount(order, captureAmount));

            return new OrderCaptureInfo(OrderCaptureState.Success, PreparedMessages.CaptureSuccessMessage);
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Capture failed: {0}", ex.Message);
            return new OrderCaptureInfo(OrderCaptureState.Failed, "Capture failed");
        }
    }

    private OrderCaptureInfo CaptureFailed(Order? order, string message)
    {
        LogError(order, "Capture failed: {0}", message);
        return new OrderCaptureInfo(OrderCaptureState.Failed, message);
    }

    /// <summary>
    /// Voids the transaction. Only possible until the batch settles (same day).
    /// </summary>
    public bool CancelOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        try
        {
            LogEvent(order, "Cancel (void) requested");

            string errorText = OrderHelper.GetOrderError(order);
            if (!string.IsNullOrEmpty(errorText))
            {
                LogError(order, "Void failed: {0}", errorText);
                return false;
            }

            AuthorizeNetService service = GetAuthorizeNetService(order);
            CreateTransactionResponse? response = service.Void(order);

            if (IsResponseSuccessful(response, order))
            {
                order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(TransactionStatusEnum.Voided);
                Save(order);
                LogEvent(order, "Void transaction successful");
                return true;
            }

            LogError(order, "Void failed: {0}", GetErrorText(response) ?? "Void transaction failed");
            return false;
        }
        catch (Exception ex)
        {
            LogError(order, ex, "{0}: {1}", PreparedMessages.UnexpectedErrorMessage, ex.Message);
            return false;
        }
    }

    public void FullReturn(Order order) => ProceedReturn(order, null);

    public void PartialReturn(Order order, Order originalOrder) => ProceedReturn(originalOrder, order?.Price?.PricePIP);

    private void ProceedReturn(Order? order, long? amountPIP)
    {
        if (order is null)
        {
            LogError(null, PreparedMessages.OrderNotSetMessage);
            return;
        }

        string errorText = OrderHelper.GetOrderError(order);
        if (!string.IsNullOrEmpty(errorText))
        {
            ReturnFailed(order, errorText, 0);
            return;
        }

        if (order.CaptureInfo is null || order.CaptureInfo.State is not (OrderCaptureState.Success or OrderCaptureState.Split) || order.CaptureAmount <= 0)
        {
            ReturnFailed(order, "Order must be captured before return.", 0);
            return;
        }

        double previouslyRefunded = GetRefundedAmount(order);
        double remainingRefundable = order.CaptureAmount - previouslyRefunded;
        double operationAmount = amountPIP is null ? remainingRefundable : Converter.ToDouble(amountPIP) / 100;

        if (operationAmount <= OrderHelper.AmountTolerance)
        {
            ReturnFailed(order, $"Nothing left to refund. Captured: {FormatAmount(order, order.CaptureAmount)}, already refunded: {FormatAmount(order, previouslyRefunded)}.", 0);
            return;
        }

        if (operationAmount > remainingRefundable + OrderHelper.AmountTolerance)
        {
            ReturnFailed(order, $"Remaining refundable amount ({FormatAmount(order, remainingRefundable)}) is less than the amount requested ({FormatAmount(order, operationAmount)}).", operationAmount);
            return;
        }

        if (string.IsNullOrWhiteSpace(order.TransactionCardNumber) || order.TransactionCardNumber.Length < 4)
        {
            ReturnFailed(order, "The last four digits of the card number are required for a refund. They are stored when the capture webhook arrives.", operationAmount);
            return;
        }

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            CreateTransactionResponse? response = service.Refund(order, operationAmount);

            if (!IsResponseSuccessful(response, order))
            {
                ReturnFailed(order, GetErrorText(response) ?? "Refund failed", operationAmount);
                return;
            }

            string refundTransId = response.TransactionResponse?.TransId ?? "";
            bool isFullRefund = OrderHelper.AmountsEqual(previouslyRefunded + operationAmount, order.CaptureAmount);
            OrderReturnOperationState operationState = isFullRefund ? OrderReturnOperationState.FullyReturned : OrderReturnOperationState.PartiallyReturned;

            string refundMessage = string.IsNullOrEmpty(refundTransId)
                ? PreparedMessages.RefundSuccessMessage
                : $"{PreparedMessages.RefundSuccessMessage} {string.Format(RefundTransactionMarkerFormat, refundTransId)}";

            OrderReturnInfo.SaveReturnOperation(operationState, refundMessage, operationAmount, order);

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(TransactionStatusEnum.RefundPendingSettlement);
            Save(order);
            LogEvent(order, "Refunded {0} (transaction {1})", FormatAmount(order, operationAmount), refundTransId);
        }
        catch (Exception ex)
        {
            string errorMessage = $"{PreparedMessages.UnexpectedErrorMessage}: {ex.Message}" +
                " Note: Authorize.Net can only refund transactions that have settled. Unsettled transactions must be voided instead.";

            LogError(order, ex, "{0}", errorMessage);
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, errorMessage, 0, order);
        }
    }

    private void ReturnFailed(Order order, string message, double amount)
    {
        LogError(order, "Refund failed: {0}", message);
        OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed, message, amount, order);
    }

    private static double GetRefundedAmount(Order order)
        => order.ReturnOperations?
            .Where(operation => operation.State is OrderReturnOperationState.PartiallyReturned or OrderReturnOperationState.FullyReturned)
            .Sum(operation => operation.Amount) ?? 0;

    #endregion

    #region Webhook callbacks (ICheckoutHandlerCallback)

    /// <summary>
    /// Resolves the order a webhook notification belongs to. The refId sent with every request comes
    /// back as merchantReferenceId; invoiceNumber is the fallback.
    /// </summary>
    public static Order GetOrderFromCallback(CallbackData data)
    {
        // The interface declares a non-nullable Order; Dynamicweb treats null as "no order for this callback"
        // and the request ends without reaching HandleCallback.
        return TryGetOrderFromCallback(data)!;
    }

    private static Order? TryGetOrderFromCallback(CallbackData data)
    {
        var logger = new AuthorizeNetLogger(debugLogging: true);

        try
        {
            NotificationItem? notification = WebhookPayloadReader.Parse(data.Body);
            if (notification is null)
            {
                logger.LogError("Invalid webhook payload: missing notification data");
                return null;
            }

            NotificationPayload? payload = notification.Payload;
            if (payload is null || !WebhookPayloadReader.TryGetOrderId(notification, out string? orderId))
            {
                logger.LogError("Webhook {0} for transaction {1} carries neither merchantReferenceId nor invoiceNumber", notification.EventType, notification.Payload?.Id);
                return null;
            }

            Order? order = FindOrder(payload.MerchantReferenceId) ?? FindOrder(payload.InvoiceNumber);
            if (order is null)
            {
                logger.LogError("Order not found for webhook reference: {0}", orderId);
                return null;
            }

            order.GatewayResult = data.Body;

            return order;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract order from callback data: {0}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Finds an order by the reference Authorize.Net echoes back: the numeric AutoId sent as refId, or an order id.
    /// </summary>
    private static Order? FindOrder(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return null;

        reference = reference.Trim();

        return int.TryParse(reference, out int autoId)
            ? Ecommerce.Services.Orders.GetByAutoId(autoId)
            : Ecommerce.Services.Orders.GetById(reference);
    }

    /// <summary>
    /// Processes a webhook notification. The signature is verified first; the notification is only a trigger,
    /// the transaction data is always re-read from the API.
    /// </summary>
    public OutputResult HandleCallback(Order order, CallbackData data)
    {
        if (order is null)
        {
            LogError(null, "Webhook processing skipped. Order is not set");
            return ContentOutputResult.Empty;
        }

        try
        {
            LogEvent(order, "Webhook callback received");

            if (string.IsNullOrWhiteSpace(SignatureKey))
            {
                LogError(order, "Cannot handle webhook: Signature key not configured");
                return ContentOutputResult.Empty;
            }

            string? signature = data.Headers
                .FirstOrDefault(header => header.Key.Equals(SignatureHeaderName, StringComparison.OrdinalIgnoreCase))
                .Value?.FirstOrDefault();

            if (!HmacValidator.IsValid(SignatureKey, data.Body, signature))
            {
                LogError(order, "Webhook signature validation failed. Signature present: {0}", !string.IsNullOrEmpty(signature));
                return ContentOutputResult.Empty;
            }

            order.GatewayResult = string.Empty;
            Ecommerce.Services.Orders.UpdateGatewayResult(order, false);

            NotificationItem? notification = WebhookPayloadReader.Parse(data.Body);
            if (notification?.Payload is null)
            {
                LogError(order, "Invalid webhook payload: missing notification data");
                return ContentOutputResult.Empty;
            }

            NotificationPayload payload = notification.Payload;
            NotificationEventType? eventType = notification.GetEventType();

            LogEvent(order, "Processing webhook event {0} for transaction {1}", notification.EventType, payload.Id);

            switch (eventType)
            {
                case NotificationEventType.AuthCaptureCreated:
                case NotificationEventType.AuthCreated:
                    HandleAuthOrCaptureCallback(order, payload, eventType is NotificationEventType.AuthCaptureCreated);
                    break;

                case NotificationEventType.CaptureCreated:
                case NotificationEventType.PriorAuthCaptureCreated:
                    HandleCaptureCallback(order, payload);
                    break;

                case NotificationEventType.RefundCreated:
                    HandleRefundCallback(order, payload);
                    break;

                case NotificationEventType.VoidCreated:
                    HandleVoidCallback(order, payload);
                    break;

                case NotificationEventType.FraudHeld:
                    HandleHeldForReview(order, payload.Id);
                    break;

                case NotificationEventType.FraudApproved:
                    HandleFraudApproved(order, payload.Id);
                    break;

                case NotificationEventType.FraudDeclined:
                    HandleFraudDeclined(order, payload.Id);
                    break;

                default:
                    LogEvent(order, "Unhandled webhook event type: {0}", notification.EventType);
                    break;
            }

            LogEvent(order, "Webhook callback processing completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Webhook callback processing failed: {0}", ex.Message);
        }

        return ContentOutputResult.Empty;
    }

    private void HandleAuthOrCaptureCallback(Order order, NotificationPayload payload, bool isCaptured)
    {
        if (payload.ResponseCode == ResponseCodes.HeldForReview)
        {
            HandleHeldForReview(order, payload.Id);
            return;
        }

        if (payload.ResponseCode != ResponseCodes.Approved)
        {
            order.TransactionStatus = $"Authorisation failed: {TransactionDetailsHelper.GetResponseTextByCode(payload.ResponseCode)}";
            Save(order);
            return;
        }

        if (isCaptured)
        {
            SynchronizeOrderWithCapture(order, payload.Id);
            return;
        }

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType transactionDetails = service.GetTransactionDetails(payload.Id);

            UpdateBasicTransactionInfo(order, transactionDetails);
            UpdateCardInformation(order, transactionDetails);

            double orderAmount = OrderHelper.GetOrderAmount(order);
            double authAmount = transactionDetails.AuthAmount > 0 ? transactionDetails.AuthAmount : orderAmount;
            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(transactionDetails.TransactionStatus);

            if (!OrderHelper.AmountsEqual(authAmount, orderAmount))
            {
                // Same rule as the receipt path: an authorization that does not cover the order is not a payment.
                LogError(order, "Authorized amount {0} does not match the order amount {1}; the order is left incomplete for review",
                    FormatAmount(order, authAmount), FormatAmount(order, orderAmount));
                Save(order);
                return;
            }

            if (!OrderHelper.AmountsEqual(order.TransactionAmount, authAmount))
            {
                order.TransactionAmount = authAmount;
                LogEvent(order, "Updated authorization amount: {0}", FormatAmount(order, authAmount));
            }

            CompleteOrderFromWebhook(order, transactionDetails.TransId);
            Save(order);
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to process authorization callback: {0}", ex.Message);
        }
    }

    /// <summary>
    /// A transaction held by the Authorize.Net fraud filters. Same outcome as on the receipt path: the order
    /// is completed with the status "Held for Review" so the merchant can approve or decline it in the
    /// Merchant Interface; the fraud.approved / fraud.declined webhooks finish the story.
    /// </summary>
    private void HandleHeldForReview(Order order, string transactionId)
    {
        order.TransactionStatus = TransactionDetailsHelper.GetResponseTextByCode(ResponseCodes.HeldForReview);
        OrderHelper.UpdateTransactionNumber(order, transactionId);

        try
        {
            TransactionDetailsType transactionDetails = GetAuthorizeNetService(order).GetTransactionDetails(transactionId);
            UpdateCardInformation(order, transactionDetails);
            double amount = transactionDetails.AuthAmount > 0 ? transactionDetails.AuthAmount : transactionDetails.SettleAmount;
            if (amount > 0)
                order.TransactionAmount = amount;
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Held transaction {0} could not be read; amount and card data are updated when the review completes: {1}", transactionId, ex.Message);
        }

        LogEvent(order, "Transaction {0} is held for review", transactionId);
        CompleteOrderFromWebhook(order, transactionId);
        Save(order);
    }

    /// <summary>
    /// The merchant approved a held transaction. The transaction is re-read: an authorization-and-capture
    /// transaction is now captured, an authorization-only one is pending capture.
    /// </summary>
    private void HandleFraudApproved(Order order, string transactionId)
    {
        try
        {
            TransactionDetailsType transactionDetails = GetAuthorizeNetService(order).GetTransactionDetails(transactionId);

            LogEvent(order, "Held transaction {0} approved; status {1}", transactionId, transactionDetails.TransactionStatus);
            UpdateBasicTransactionInfo(order, transactionDetails);
            UpdateCardInformation(order, transactionDetails);

            if (transactionDetails.SettleAmount > 0
                && transactionDetails.TransactionStatus is TransactionStatusEnum.CapturedPendingSettlement or TransactionStatusEnum.SettledSuccessfully)
            {
                HandleCaptureDelta(order, transactionDetails.SettleAmount, transactionDetails.TransId);
            }

            if (transactionDetails.AuthAmount > 0)
                order.TransactionAmount = transactionDetails.AuthAmount;

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(transactionDetails.TransactionStatus);
            CompleteOrderFromWebhook(order, transactionDetails.TransId);
            Save(order);
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to process fraud approval for transaction {0}: {1}", transactionId, ex.Message);
        }
    }

    /// <summary>
    /// The merchant declined a held transaction: no money moves, the order keeps its history.
    /// </summary>
    private void HandleFraudDeclined(Order order, string transactionId)
    {
        order.TransactionStatus = "Declined after review";
        if (order.CaptureInfo?.State is OrderCaptureState.Success or OrderCaptureState.Split)
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Failed, "Transaction declined by the merchant after fraud review");

        Save(order);
        LogEvent(order, "Held transaction {0} was declined after review", transactionId);
    }

    private void HandleCaptureCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != ResponseCodes.Approved)
        {
            order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Failed, $"Capture failed: {TransactionDetailsHelper.GetResponseTextByCode(payload.ResponseCode)}");
            Save(order);
            return;
        }

        SynchronizeOrderWithCapture(order, payload.Id);
    }

    private void HandleRefundCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != ResponseCodes.Approved)
        {
            OrderReturnInfo.SaveReturnOperation(OrderReturnOperationState.Failed,
                $"Order refund failed: {TransactionDetailsHelper.GetResponseTextByCode(payload.ResponseCode)}", 0, order);
            return;
        }

        SynchronizeOrderWithRefund(order, payload.Id);
    }

    private void HandleVoidCallback(Order order, NotificationPayload payload)
    {
        if (payload.ResponseCode != ResponseCodes.Approved)
        {
            // A failed void leaves the capture state as it was.
            LogError(order, "Void transaction {0} was not approved: {1}", payload.Id, TransactionDetailsHelper.GetResponseTextByCode(payload.ResponseCode));
            return;
        }

        SynchronizeOrderWithVoid(order, payload.Id);
    }

    /// <summary>
    /// Re-reads a capture transaction from the API and records any newly captured amount.
    /// </summary>
    private void SynchronizeOrderWithCapture(Order order, string transactionId)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType transactionDetails = service.GetTransactionDetails(transactionId);

            LogEvent(order, "Synchronizing order with capture transaction {0}", transactionId);
            UpdateBasicTransactionInfo(order, transactionDetails);
            UpdateCardInformation(order, transactionDetails);

            if (transactionDetails.SettleAmount > 0)
                HandleCaptureDelta(order, transactionDetails.SettleAmount, transactionDetails.TransId);

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(transactionDetails.TransactionStatus);

            Save(order);
            LogEvent(order, "Capture synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize capture with API data: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Re-reads a refund transaction from the API. Each refund is its own transaction; its settleAmount
    /// is the amount of that refund, not a running total.
    /// </summary>
    private void SynchronizeOrderWithRefund(Order order, string transactionId)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType transactionDetails = service.GetTransactionDetails(transactionId);

            LogEvent(order, "Synchronizing order with refund transaction {0}", transactionId);

            if (transactionDetails.SettleAmount > 0)
                HandleRefundDelta(order, transactionDetails.SettleAmount, transactionDetails.TransId);

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(transactionDetails.TransactionStatus);

            Save(order);
            LogEvent(order, "Refund synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize refund with API data: {0}", ex.Message);
        }
    }

    private void SynchronizeOrderWithVoid(Order order, string transactionId)
    {
        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            TransactionDetailsType transactionDetails = service.GetTransactionDetails(transactionId);

            LogEvent(order, "Synchronizing order with void transaction {0}", transactionId);
            UpdateBasicTransactionInfo(order, transactionDetails);

            if (order.CaptureInfo?.State is OrderCaptureState.Cancel)
                LogEvent(order, "Transaction already voided");
            else
                order.CaptureInfo = new OrderCaptureInfo(OrderCaptureState.Cancel, "Transaction voided");

            order.TransactionStatus = TransactionDetailsHelper.GetTransactionStatus(transactionDetails.TransactionStatus);

            Save(order);
            LogEvent(order, "Void synchronization completed");
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Failed to synchronize void with API data: {0}", ex.Message);
        }
    }

    private static void UpdateBasicTransactionInfo(Order order, TransactionDetailsType transactionDetails)
    {
        if (!string.IsNullOrEmpty(transactionDetails.TransId))
            OrderHelper.UpdateTransactionNumber(order, transactionDetails.TransId);
    }

    /// <summary>
    /// Stores card type and masked number when the order does not have them yet (hosted-form payments),
    /// and finishes a pending "save card" request once the transaction number is known.
    /// </summary>
    private void UpdateCardInformation(Order order, TransactionDetailsType transactionDetails)
    {
        CreditCardMaskedType? creditCard = transactionDetails.Payment?.CreditCard;
        if (string.IsNullOrEmpty(order.TransactionCardNumber) && creditCard is not null)
        {
            order.TransactionCardType = creditCard.CardType?.ToString();
            order.TransactionCardNumber = creditCard.CardNumber;
        }

        if (order.GatewayPaymentStatus?.StartsWith(SecuritySettings.SavedCardNamePlaceholder, StringComparison.Ordinal) is true
            && !string.IsNullOrEmpty(order.TransactionNumber))
        {
            string cardName = order.GatewayPaymentStatus.Substring(SecuritySettings.SavedCardNamePlaceholder.Length);
            order.GatewayPaymentStatus = string.Empty;
            SaveCard(order, cardName);
        }
    }

    /// <summary>
    /// Records a captured amount reported by the API when it exceeds what the order already has.
    /// </summary>
    private void HandleCaptureDelta(Order order, double apiCaptureAmount, string? transactionId)
    {
        double currentCaptureAmount = order.CaptureAmount;
        double captureDelta = apiCaptureAmount - currentCaptureAmount;

        LogEvent(order, "Capture delta check: API={0}, Current={1}, Delta={2}", DebuggingInfoType.CaptureResult,
            FormatAmount(order, apiCaptureAmount), FormatAmount(order, currentCaptureAmount), FormatAmount(order, captureDelta));

        if (captureDelta <= OrderHelper.AmountTolerance)
        {
            LogEvent(order, "No new capture amount detected", DebuggingInfoType.CaptureResult);
            return;
        }

        double authAmount = OrderHelper.GetOrderAmount(order);
        bool isPartialCapture = !OrderHelper.AmountsEqual(apiCaptureAmount, authAmount);
        OrderCaptureState captureState = isPartialCapture ? OrderCaptureState.Split : OrderCaptureState.Success;

        string message = isPartialCapture
            ? $"Partial capture: {FormatAmount(order, captureDelta)} (total {FormatAmount(order, apiCaptureAmount)} of {FormatAmount(order, authAmount)})"
            : $"Capture: {FormatAmount(order, apiCaptureAmount)}";

        order.CaptureInfo = new OrderCaptureInfo(captureState, message);
        order.CaptureAmount = apiCaptureAmount;
        order.TransactionAmount = apiCaptureAmount;

        UpdateReturnOperationStatesAfterCapture(order, apiCaptureAmount);

        LogEvent(order, "Processed capture: {0}. Amount changed from {1} to {2}", DebuggingInfoType.CaptureResult,
            message, FormatAmount(order, currentCaptureAmount), FormatAmount(order, apiCaptureAmount));

        if (captureState == OrderCaptureState.Success)
            CompleteOrderFromWebhook(order, transactionId);
    }

    /// <summary>
    /// Completes an order from a webhook when the customer never returned from the hosted page.
    /// </summary>
    private void CompleteOrderFromWebhook(Order order, string? transactionId)
    {
        if (order.Complete)
            return;

        // Another request (the customer returning from the hosted page) may have completed the order meanwhile.
        Order? current = Ecommerce.Services.Orders.GetById(order.Id);
        if (current?.Complete == true)
        {
            LogEvent(order, "Order was completed by another request; skipping completion from webhook");
            return;
        }

        LogEvent(order, "Completing order from webhook (transaction {0})", transactionId ?? order.TransactionNumber);
        MarkOrderComplete(order, transactionId ?? order.TransactionNumber);
        CheckoutDone(order);
    }

    private void MarkOrderComplete(Order order, string? transactionNumber)
    {
        if (string.IsNullOrWhiteSpace(transactionNumber))
            SetOrderComplete(order);
        else
            SetOrderComplete(order, transactionNumber);
    }

    /// <summary>
    /// Records one refund reported by a webhook, unless that refund transaction was already recorded by
    /// FullReturn/PartialReturn (marker in the return operation message).
    /// </summary>
    private void HandleRefundDelta(Order order, double refundOperationAmount, string? transactionId)
    {
        double capturedAmount = order.CaptureAmount;

        if (capturedAmount <= 0)
        {
            LogEvent(order, "Skipping refund - no captured amount to refund", DebuggingInfoType.ReturnResult);
            return;
        }

        if (refundOperationAmount <= OrderHelper.AmountTolerance)
        {
            LogEvent(order, "Skipping refund - amount is zero: {0}", DebuggingInfoType.ReturnResult, FormatAmount(order, refundOperationAmount));
            return;
        }

        if (!string.IsNullOrEmpty(transactionId))
        {
            string marker = string.Format(RefundTransactionMarkerFormat, transactionId);
            bool alreadyRecorded = order.ReturnOperations?.Any(operation => operation.Message?.Contains(marker, StringComparison.Ordinal) == true) == true;

            if (alreadyRecorded)
            {
                LogEvent(order, "Skipping refund - transaction {0} was already recorded", DebuggingInfoType.ReturnResult, transactionId);
                return;
            }
        }

        double previousRefundedAmount = GetRefundedAmount(order);
        double totalRefundedAfterOperation = previousRefundedAmount + refundOperationAmount;

        LogEvent(order, "Refund check: this refund={0}, previously refunded={1}, captured={2}", DebuggingInfoType.ReturnResult,
            FormatAmount(order, refundOperationAmount), FormatAmount(order, previousRefundedAmount), FormatAmount(order, capturedAmount));

        if (totalRefundedAfterOperation > capturedAmount + OrderHelper.AmountTolerance)
        {
            LogEvent(order, "Skipping refund - total {0} would exceed captured amount {1}", DebuggingInfoType.ReturnResult,
                FormatAmount(order, totalRefundedAfterOperation), FormatAmount(order, capturedAmount));
            return;
        }

        bool isPartialRefund = totalRefundedAfterOperation < capturedAmount - OrderHelper.AmountTolerance;
        OrderReturnOperationState refundState = isPartialRefund ? OrderReturnOperationState.PartiallyReturned : OrderReturnOperationState.FullyReturned;

        string markerSuffix = string.IsNullOrEmpty(transactionId) ? "" : $" {string.Format(RefundTransactionMarkerFormat, transactionId)}";
        string message = isPartialRefund
            ? $"Partial refund: {FormatAmount(order, refundOperationAmount)} (total refunded {FormatAmount(order, totalRefundedAfterOperation)} of {FormatAmount(order, capturedAmount)}){markerSuffix}"
            : $"Full refund: {FormatAmount(order, refundOperationAmount)} (total refunded {FormatAmount(order, totalRefundedAfterOperation)}){markerSuffix}";

        OrderReturnInfo.SaveReturnOperation(refundState, message, refundOperationAmount, order);

        LogEvent(order, "Processed refund: {0}", DebuggingInfoType.ReturnResult, message);
    }

    /// <summary>
    /// When the captured amount grows after a refund was recorded as "fully returned", that refund is
    /// only partial now.
    /// </summary>
    private void UpdateReturnOperationStatesAfterCapture(Order order, double totalCapturedAmount)
    {
        if (order.ReturnOperations is null || !order.ReturnOperations.Any())
            return;

        // Work on a snapshot and assign it back: the property may be materialized from storage on every read.
        List<OrderReturnInfo> operations = order.ReturnOperations.ToList();
        List<OrderReturnInfo> fullyReturnedOperations = operations
            .Where(operation => operation.State is OrderReturnOperationState.FullyReturned)
            .ToList();

        if (fullyReturnedOperations.Count == 0)
            return;

        double totalRefundedAmount = GetRefundedAmount(order);
        if (totalCapturedAmount <= totalRefundedAmount + OrderHelper.AmountTolerance)
            return;

        foreach (OrderReturnInfo orderReturnInfo in fullyReturnedOperations)
            orderReturnInfo.State = OrderReturnOperationState.PartiallyReturned;

        order.ReturnOperations = operations;

        LogEvent(order, "Updated {0} return operations after new capture. Total captured: {1}, total refunded: {2}", DebuggingInfoType.CaptureResult,
            fullyReturnedOperations.Count, FormatAmount(order, totalCapturedAmount), FormatAmount(order, totalRefundedAmount));
    }

    #endregion

    #region Webhook registration

    /// <summary>
    /// Registers the webhook subscription when auto-registration is enabled. The check runs at most once
    /// per hour per account and URL. Failures never block the checkout.
    /// </summary>
    private void TryRegisterWebhooks(Order order)
    {
        if (!EnableAutoWebhookRegistration && !ForceWebhookRegistration)
            return;

        string webhookUrl;
        try
        {
            webhookUrl = BuildWebhookUrl(order);
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Webhook URL could not be built, skipping registration: {0}", ex.Message);
            return;
        }

        string cacheKey = $"{TestMode}|{ApiLoginId}|{webhookUrl}";
        bool force = ForceWebhookRegistration;

        if (!force && WebhookChecks.TryGetValue(cacheKey, out DateTime checkedAt) && DateTime.UtcNow - checkedAt < WebhookCheckInterval)
            return;

        try
        {
            AuthorizeNetService service = GetAuthorizeNetService(order);
            WebhookResponse? created = service.EnsureWebhooksRegistered(webhookUrl, force);
            WebhookChecks[cacheKey] = DateTime.UtcNow;

            if (created is not null)
                LogEvent(order, "Webhook registered for {0}. Webhook id: {1}", webhookUrl, created.WebhookId);
            else
                LogEvent(order, "Webhook for {0} is already configured", webhookUrl);

            if (force)
            {
                ForceWebhookRegistration = false;
                PersistParameters(order);
            }
        }
        catch (Exception ex)
        {
            LogError(order, ex, "Webhook registration failed, continuing with payment: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Writes the current parameters back to the payment method (used to clear the force flag).
    /// </summary>
    private void PersistParameters(Order order)
    {
        var payment = string.IsNullOrEmpty(order.PaymentMethodId) ? null : Ecommerce.Services.Payments.GetPayment(order.PaymentMethodId);
        if (payment is null)
        {
            LogError(order, "Payment method {0} not found; parameters were not saved", order.PaymentMethodId);
            return;
        }

        payment.CheckoutParameters = GetParametersToXml();
        Ecommerce.Services.Payments.Save(payment);
    }

    /// <summary>
    /// Callback URL of the DW10 checkout handler callback endpoint: /dwapi/ecommerce/carts/callback/{handler name}
    /// </summary>
    private string BuildWebhookUrl(Order order)
    {
        var baseUri = new Uri(GetBaseUrl(order));
        string handlerName = AddInManager.GetAddInName(GetType());

        return $"{baseUri.Scheme}://{baseUri.Authority}/dwapi/ecommerce/carts/callback/{Uri.EscapeDataString(handlerName)}";
    }

    #endregion

    #region Infrastructure

    private AuthorizeNetService GetAuthorizeNetService(Order? order)
        => new(ApiLoginId, TransactionKey, TestMode, DebugLogging, order);

    private static void Save(Order order) => Ecommerce.Services.Orders.Save(order);

    private static string FormatAmount(Order order, double amount)
        => Ecommerce.Services.Currencies.Format(order.Currency, amount);

    private static string? GetErrorText(CreateTransactionResponse? response)
        => response?.TransactionResponse?.Errors?.FirstOrDefault()?.ErrorText
           ?? response?.Messages?.Message?.FirstOrDefault()?.Text;

    /// <summary>
    /// True when createTransactionRequest returned an approved transaction.
    /// </summary>
    private bool IsResponseSuccessful([NotNullWhen(true)] CreateTransactionResponse? response, Order? order)
    {
        if (response?.TransactionResponse is null)
        {
            LogError(order, "No transaction response received from Authorize.Net");
            return false;
        }

        bool isSuccess = Converter.ToInt32(response.TransactionResponse.ResponseCode) == ResponseCodes.Approved;

        if (!isSuccess)
        {
            LogError(order, "Transaction not approved: {0} (response code {1})",
                GetErrorText(response) ?? "Transaction failed", response.TransactionResponse.ResponseCode ?? "");
        }

        return isSuccess;
    }

    private bool IsResponseSuccessful([NotNullWhen(true)] GetHostedPaymentPageResponse? response, Order? order)
    {
        if (response is null)
        {
            LogError(order, "No hosted payment page response received from Authorize.Net");
            return false;
        }

        bool isSuccess = AuthorizeNetService.IsOk(response.Messages) && !string.IsNullOrWhiteSpace(response.Token);

        if (!isSuccess)
            LogError(order, "Hosted payment page creation failed: {0}", response.Messages?.Message?.FirstOrDefault()?.Text ?? "no token returned");

        return isSuccess;
    }

    /// <summary>
    /// What the handler needs to know about an accepted payment, independent of which API call reported it.
    /// </summary>
    private sealed record PaymentOutcome(
        string TransId,
        int ResponseCode,
        TransactionStatusEnum? TransactionStatus,
        string? AccountType,
        string? AccountNumber,
        double Amount,
        bool IsCaptured)
    {
        public static PaymentOutcome FromTransactionResponse(TransactionResponse? response, double amount, TransactionType transactionType)
        {
            int responseCode = Converter.ToInt32(response?.ResponseCode);

            return new PaymentOutcome(
                response?.TransId ?? "",
                responseCode,
                null,
                response?.AccountType,
                response?.AccountNumber,
                amount,
                responseCode == ResponseCodes.Approved && transactionType is TransactionType.AuthCaptureTransaction);
        }

        public static PaymentOutcome FromTransactionDetails(TransactionDetailsType details)
        {
            bool captured = details.TransactionStatus is TransactionStatusEnum.CapturedPendingSettlement
                or TransactionStatusEnum.SettledSuccessfully
                or TransactionStatusEnum.PendingSettlement
                or TransactionStatusEnum.PendingFinalSettlement;

            return new PaymentOutcome(
                details.TransId,
                details.ResponseCode,
                details.TransactionStatus,
                details.Payment?.CreditCard?.CardType?.ToString(),
                details.Payment?.CreditCard?.CardNumber,
                details.AuthAmount > 0 ? details.AuthAmount : details.SettleAmount,
                captured);
        }
    }

    #endregion
}
