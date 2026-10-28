using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Constants;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Exceptions;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Services;

/// <summary>
/// Sends requests to the Authorize.Net JSON API and the Webhooks REST API.
/// One pooled HttpClient per environment is shared by all instances.
/// </summary>
internal sealed class AuthorizeNetHttpService
{
    private static readonly HttpClient ProductionHttpClient = CreateHttpClient(AuthorizeNetEndpoints.GetWebhookBaseUrl(isTestMode: false));
    private static readonly HttpClient SandboxHttpClient = CreateHttpClient(AuthorizeNetEndpoints.GetWebhookBaseUrl(isTestMode: true));

    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly AuthorizeNetLogger _logger;

    public AuthorizeNetHttpService(bool isTestMode, AuthorizeNetLogger logger)
    {
        _apiUrl = AuthorizeNetEndpoints.GetApiEndpoint(isTestMode);
        _logger = logger;
        _httpClient = isTestMode ? SandboxHttpClient : ProductionHttpClient;
    }

    /// <summary>
    /// Posts a request to the JSON API endpoint (createTransactionRequest, getTransactionDetailsRequest, ...).
    /// </summary>
    public T? Post<T>(string jsonObject)
        => SendRequest<T>(HttpMethod.Post, _apiUrl, jsonObject, null);

    /// <summary>
    /// Posts to a REST endpoint (relative to the environment base URL) with custom headers.
    /// </summary>
    public T? Post<T>(string endpoint, string jsonObject, IReadOnlyDictionary<string, string>? headers)
        => SendRequest<T>(HttpMethod.Post, endpoint, jsonObject, headers);

    public T? Get<T>(string endpoint, IReadOnlyDictionary<string, string>? headers)
        => SendRequest<T>(HttpMethod.Get, endpoint, null, headers);

    public void Delete(string endpoint, IReadOnlyDictionary<string, string>? headers)
        => SendRequest<object>(HttpMethod.Delete, endpoint, null, headers);

    private T? SendRequest<T>(HttpMethod method, string endpoint, string? jsonObject, IReadOnlyDictionary<string, string>? headers)
    {
        var requestLogger = new AuthorizeNetRequestLogger(_logger);
        string responseText = string.Empty;

        try
        {
            using var requestMessage = new HttpRequestMessage(method, endpoint);

            if (jsonObject is not null)
            {
                requestMessage.Content = new StringContent(jsonObject, Encoding.UTF8);
                requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            if (headers is not null)
            {
                foreach ((string name, string value) in headers)
                    requestMessage.Headers.TryAddWithoutValidation(name, value);
            }

            requestLogger.LogRequest(requestMessage, jsonObject ?? "");

            using HttpResponseMessage response = _httpClient.Send(requestMessage, HttpCompletionOption.ResponseContentRead);
            responseText = ReadBody(response);
            requestLogger.LogResponse(response, responseText);

            if (method == HttpMethod.Delete)
            {
                if (!response.IsSuccessStatusCode)
                    throw new AuthorizeNetApiException($"DELETE {endpoint} failed with status {(int)response.StatusCode} {response.ReasonPhrase}");

                return default;
            }

            if (TryParseError(responseText) is ErrorResponse errorResponse)
                throw new AuthorizeNetApiException($"Authorize.Net API error: {DescribeError(errorResponse, responseText)}", errorResponse);

            if (!response.IsSuccessStatusCode)
                throw new AuthorizeNetApiException($"{method} {endpoint} failed with status {(int)response.StatusCode} {response.ReasonPhrase}");

            if (string.IsNullOrWhiteSpace(responseText))
                return default;

            return AuthorizeNetJson.Deserialize<T>(responseText);
        }
        catch (AuthorizeNetApiException ex)
        {
            requestLogger.LogException(ex);
            throw;
        }
        catch (Exception ex)
        {
            requestLogger.LogException(ex);
            throw new AuthorizeNetApiException($"Authorize.Net request failed: {ex.Message}", ex);
        }
        finally
        {
            requestLogger.FinalizeLog();
        }
    }

    /// <summary>
    /// Reads the body as UTF-8. Authorize.Net prefixes JSON responses with a byte order mark, which the
    /// reader strips.
    /// </summary>
    private static string ReadBody(HttpResponseMessage response)
    {
        using Stream stream = response.Content.ReadAsStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd().TrimStart('﻿');
    }

    private static string DescribeError(ErrorResponse errorResponse, string responseText)
    {
        string messages = string.Join("; ",
            errorResponse.Messages?.Message?.Select(m => $"[{m.Code}] {m.Text}") ?? ["Unknown error"]);

        if (AuthorizeNetJson.TryDeserialize(responseText, out CreateTransactionResponse? transactionWrapper)
            && transactionWrapper.TransactionResponse?.Errors is { } errors && errors.Any())
        {
            messages += " Details: " + string.Join("; ", errors.Select(e => $"[{e.ErrorCode}] {e.ErrorText}"));
        }

        return messages;
    }

    /// <summary>
    /// Returns the parsed error when the response carries resultCode "Error", otherwise null.
    /// </summary>
    internal static ErrorResponse? TryParseError(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return null;

        if (AuthorizeNetJson.TryDeserialize(responseText, out ErrorResponse? errorResponse)
            && DataContractEnumConverter<MessageTypeEnum>.TryParse(errorResponse.Messages?.ResultCode, out MessageTypeEnum resultCode)
            && resultCode is MessageTypeEnum.Error)
        {
            return errorResponse;
        }

        return null;
    }

    private static HttpClient CreateHttpClient(string baseAddress)
    {
        var handler = new SocketsHttpHandler
        {
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AutomaticDecompression = System.Net.DecompressionMethods.All
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(90)
        };
    }
}
