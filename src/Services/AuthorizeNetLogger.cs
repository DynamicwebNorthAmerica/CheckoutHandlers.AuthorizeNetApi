using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Logging;
using System;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

/// <summary>
/// Logger for Authorize.Net operations. Errors are always written; information is written only when
/// debug logging is enabled. When an order is attached, entries are also saved as order debugging info.
/// </summary>
internal sealed class AuthorizeNetLogger
{
    private const string OrderDebuggingSource = "Authorize.Net";

    private readonly ILogger _logger;
    private readonly bool _debugLogging;
    private readonly Order? _order;

    public AuthorizeNetLogger(bool debugLogging, Order? order = null)
    {
        _debugLogging = debugLogging;
        _order = order;
        _logger = LogManager.Current.GetLogger($"/eCom/CheckoutHandler/{typeof(AuthorizeNetCheckoutHandler).FullName}");
    }

    public bool DebugLogging => _debugLogging;

    public void LogInfo(string message)
    {
        if (!_debugLogging || string.IsNullOrWhiteSpace(message))
            return;

        _logger.Info(message);
        LogDebugOrderInfo(message);
    }

    public void LogInfo(string messageFormat, params object?[] args)
        => LogInfo(string.Format(messageFormat, args));

    public void LogError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message);
        LogDebugOrderInfo(message);
    }

    public void LogError(string messageFormat, params object?[] args)
        => LogError(string.Format(messageFormat, args));

    public void LogError(Exception exception, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        _logger.Error(message, exception);
        LogDebugOrderInfo($"{message}: {exception.Message}");
    }

    public void LogError(Exception exception, string messageFormat, params object?[] args)
        => LogError(exception, string.Format(messageFormat, args));

    private void LogDebugOrderInfo(string message)
    {
        if (_order is not null)
            Ecommerce.Services.OrderDebuggingInfos.Save(_order, message, OrderDebuggingSource);
    }
}
