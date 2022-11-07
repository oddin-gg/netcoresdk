using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Exceptions;

namespace Oddin.OddsFeedSdk.API;

internal class RestClient : IRestClient, IDisposable
{
    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(RestClient));

    private readonly string _apiHost;
    private readonly HttpClient _httpClient = new();
    private readonly Subject<IRequestResult<object>> _publisher = new();
    private readonly int _timeoutSeconds;
    private readonly bool _useSsl;

    public RestClient(IFeedConfiguration config)
    {
        _apiHost = config.ApiHost;
        _useSsl = config.UseApiSsl;
        _timeoutSeconds = config.HttpClientTimeout;

        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("x-access-token", config.AccessToken);
    }

    public void Dispose() => _publisher.OnCompleted();

    public async Task<RequestResult<TData>> SendRequestAsync<TData>(
        string route,
        HttpMethod method,
        CultureInfo culture = null,
        object objectToBeSent = null,
        (string key, object value)[] parameters = default,
        bool deserializeResponse = true,
        bool ignoreUnsuccessfulStatusCode = false
    )
        where TData : class
    {
        RequestResult<TData> result;
        try
        {
            result = await SendRequestGetResult<TData>(route, method, culture, objectToBeSent, parameters,
                deserializeResponse, ignoreUnsuccessfulStatusCode);
        }
        catch (Exception e)
        {
            _log.LogError(
                $"An exception was thrown while waiting for a result of [{method} to {CombineAddress(route)}]!");

            throw new CommunicationException(
                "An exception was thrown while waiting for a result of API request!",
                innerException: e,
                url: CombineAddress(route),
                responseCode: default,
                response: "");
        }

        if (result.Successful == false)
        {
            _log.LogError($"API request [{method} to {CombineAddress(route)}] failed. Reason: {result.Message}");

            throw new CommunicationException(
                "API request failed!",
                innerException: null,
                url: CombineAddress(route),
                responseCode: result.ResponseCode,
                response: result.RawData);
        }

        _publisher.OnNext(result);

        return result;
    }

    public RequestResult<TData> SendRequest<TData>(
        string route,
        HttpMethod method,
        CultureInfo culture = null,
        object objectToBeSent = null,
        (string key, object value)[] parameters = default,
        bool deserializeResponse = true,
        bool ignoreUnsuccessfulStatusCode = false
    )
        where TData : class
    {
        RequestResult<TData> result;
        try
        {
            result = SendRequestGetResult<TData>(route, method, culture, objectToBeSent, parameters,
                    deserializeResponse, ignoreUnsuccessfulStatusCode)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception e)
        {
            _log.LogError(
                $"An exception was thrown while waiting for a result of [{method} to {CombineAddress(route)}]!");

            throw new CommunicationException(
                "An exception was thrown while waiting for a result of API request!",
                innerException: e,
                url: CombineAddress(route),
                responseCode: default,
                response: "");
        }

        if (result.Successful == false)
        {
            _log.LogError($"API request [{method} to {CombineAddress(route)}] failed. Reason: {result.Message}");

            throw new CommunicationException(
                "API request failed!",
                innerException: null,
                url: CombineAddress(route),
                responseCode: result.ResponseCode,
                response: result.RawData);
        }

        _publisher.OnNext(result);

        return result;
    }

    public IObservable<T> SubscribeForClass<T>() => _publisher.OfType<T>();

    private async Task<RequestResult<TData>> SendRequestGetResult<TData>(
        string route,
        HttpMethod method,
        CultureInfo culture,
        object objectToBeSent = null,
        (string key, object value)[] parameters = default,
        bool deserializeResponse = true,
        bool ignoreUnsuccessfulStatusCode = false
    )
        where TData : class
    {
        if (XmlHelper.TrySerialize(objectToBeSent, out var serializedObject) == false)
            return RequestResult<TData>.Failure(failureMessage: "Request could not be serialized!");

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await SendRequestGetHttpResponse(route, method, serializedObject, parameters);
        }
        catch (ArgumentNullException)
        {
            return RequestResult<TData>.Failure(
                failureMessage: $"{nameof(ArgumentNullException)} was thrown by {nameof(HttpClient)}");
        }
        catch (InvalidOperationException)
        {
            return RequestResult<TData>.Failure(
                failureMessage: $"{nameof(InvalidOperationException)} was thrown by {nameof(HttpClient)}");
        }
        catch (HttpRequestException)
        {
            return RequestResult<TData>.Failure(
                failureMessage: $"{nameof(HttpRequestException)} was thrown by {nameof(HttpClient)}");
        }
        catch (TaskCanceledException)
        {
            return RequestResult<TData>.Failure(
                failureMessage: $"Http request timed out with timeout {_timeoutSeconds}s!");
        }

        var requestResultString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

        _log.LogTrace($"Http response: {httpResponse}, http response content: {requestResultString}");

        if (httpResponse.IsSuccessStatusCode == false)
        {
            if (ignoreUnsuccessfulStatusCode)
                return RequestResult<TData>.Success(
                    default,
                    httpResponse.StatusCode,
                    requestResultString,
                    culture: culture);
            return RequestResult<TData>.Failure(
                failureMessage:
                $"Received failure http status code: {httpResponse?.StatusCode} {httpResponse?.ReasonPhrase}",
                responseCode: httpResponse.StatusCode);
        }

        if (deserializeResponse == false)
            return RequestResult<TData>.Success(
                default,
                httpResponse.StatusCode,
                requestResultString,
                culture: culture);

        if (XmlHelper.TryDeserialize<TData>(requestResultString, out var responseDto) == false)
            return RequestResult<TData>.Failure(
                failureMessage: $"Unable to deserialize response! Response:\n{requestResultString}",
                responseCode: httpResponse.StatusCode,
                rawData: requestResultString);

        if (responseDto is null)
            return RequestResult<TData>.Failure(
                failureMessage: "Response was deserialized as null!",
                responseCode: httpResponse.StatusCode,
                rawData: requestResultString);

        return RequestResult<TData>.Success(
            responseDto,
            httpResponse.StatusCode,
            requestResultString,
            culture: culture);
    }


    private async Task<HttpResponseMessage> SendRequestGetHttpResponse(string route, HttpMethod method,
        string objectToBeSent = default, (string key, object value)[] parameters = default)
    {
        using var content = new StringContent(objectToBeSent, Encoding.UTF8, "application/xml");

        var address = CombineAddress(route);
        address = AddParametersToUrl(address, parameters);

        using var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(address)
        };

        if (method != HttpMethod.Get)
            request.Content = content;

        return await _httpClient.SendAsync(request).ConfigureAwait(false);
    }

    private string AddParametersToUrl(string address, (string key, object value)[] parameters)
    {
        if (parameters != default && parameters.Length > 0)
        {
            var parametersStr = string.Join("&",
                parameters.Select(
                    kvp => $"{kvp.key}={kvp.value}")
            );

            address = Url.Combine(address, "?", parametersStr);
        }

        return address;
    }

    private string CombineAddress(string route)
    {
        var https = string.Empty;
        if (_useSsl)
            https = "s";

        return Url.Combine($"http{https}://{_apiHost}/", route);
    }
}