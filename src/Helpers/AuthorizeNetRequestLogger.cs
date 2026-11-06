using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;
using System;
using System.Net.Http;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

/// <summary>
/// Collects one HTTP interaction (request, response, exception) into a single log entry.
/// Card numbers, card codes, transaction keys and Authorization headers are masked before logging.
/// </summary>
internal sealed class AuthorizeNetRequestLogger
{
    private readonly StringBuilder _logBuilder = new();
    private readonly AuthorizeNetLogger _logger;

    public AuthorizeNetRequestLogger(AuthorizeNetLogger logger)
    {
        _logger = logger;
    }

    public void LogRequest(HttpRequestMessage request, string requestData)
    {
        _logBuilder.AppendLine("Authorize.Net API interaction");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- REQUEST ---");
        _logBuilder.AppendLine($"Method: {request.Method}");
        _logBuilder.AppendLine($"URL: {request.RequestUri}");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- REQUEST HEADERS ---");
        _logBuilder.AppendLine(SecurityHelper.MaskSensitiveDataInContent(request.Headers.ToString()));
        _logBuilder.AppendLine("--- REQUEST DATA ---");
        _logBuilder.AppendLine(SecurityHelper.MaskSensitiveDataInContent(requestData));
    }

    public void LogResponse(HttpResponseMessage response, string responseText)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- RESPONSE ---");
        _logBuilder.AppendLine($"HttpStatusCode: {(int)response.StatusCode} ({response.ReasonPhrase})");
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- RESPONSE HEADERS ---");
        _logBuilder.AppendLine(SecurityHelper.MaskSensitiveDataInContent(response.Headers.ToString()));
        _logBuilder.AppendLine("--- RESPONSE DATA ---");
        _logBuilder.AppendLine(SecurityHelper.MaskSensitiveDataInContent(responseText));
    }

    public void LogException(Exception exception)
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine($"--- EXCEPTION ({exception.GetType().Name}) ---");
        _logBuilder.AppendLine($"Message: {SecurityHelper.MaskSensitiveDataInContent(exception.Message)}");
        _logBuilder.AppendLine($"Stack Trace: {exception.StackTrace}");
    }

    public void FinalizeLog()
    {
        _logBuilder.AppendLine();
        _logBuilder.AppendLine("--- END OF INTERACTION ---");

        _logger.LogInfo(_logBuilder.ToString());
    }
}
