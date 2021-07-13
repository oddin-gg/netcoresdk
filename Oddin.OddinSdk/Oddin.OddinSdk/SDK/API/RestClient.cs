using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.Configuration.Abstractions;
using Oddin.OddinSdk.SDK.Exceptions;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.API
{
    internal class RestClient : LoggingBase
    {
        private readonly string _apiHost;
        private readonly bool _useSsl;
        private readonly int _timeoutSeconds;
        private HttpClient _httpClient = new HttpClient();

        public RestClient(IFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _apiHost = config.ApiHost;
            _useSsl = config.UseApiSsl;
            _timeoutSeconds = config.HttpClientTimeout;

            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("x-access-token", config.AccessToken);
        }

        public async Task<RequestResult<TData>> SendRequestAsync<TData>(
            string route, 
            HttpMethod method, 
            object objectToBeSent = null, 
            (string key, object value)[] parameters = default,
            bool deserializeResponse = true
            )
            where TData : class
        {
            var result = await SendRequestGetResult<TData>(route, method, objectToBeSent, parameters, deserializeResponse);

            if (result.Successful == false)
            {
                _log.LogError($"API request [{method} to {CombineAddress(route)}] failed. Reason: {result.Message}");

                throw new CommunicationException(
                    message: "API request failed!",
                    innerException: null,
                    url: CombineAddress(route),
                    responseCode: result.ResponseCode,
                    response: result.RawData);
            }

            return result;
        }

        public RequestResult<TData> SendRequest<TData>(
            string route, 
            HttpMethod method, 
            object objectToBeSent = null, 
            (string key, object value)[] parameters = default,
            bool deserializeResponse = true
            )
            where TData : class
        {
            RequestResult<TData> result;
            try
            {
                result = SendRequestGetResult<TData>(route, method, objectToBeSent, parameters, deserializeResponse)
                    .GetAwaiter()
                    .GetResult();
            }
            catch(Exception e)
            {
                _log.LogError($"An exception was thrown while waiting for a result of [{method} to {CombineAddress(route)}]!");

                throw new CommunicationException(
                    message: "An exception was thrown while waiting for a result of API request!",
                    innerException: e,
                    url: CombineAddress(route),
                    responseCode: default,
                    response: "");
            }

            if (result.Successful == false)
            {
                _log.LogError($"API request [{method} to {CombineAddress(route)}] failed. Reason: {result.Message}");

                throw new CommunicationException(
                    message: "API request failed!",
                    innerException: null,
                    url: CombineAddress(route),
                    responseCode: result.ResponseCode,
                    response: result.RawData);
            }

            return result;
        }

        private async Task<RequestResult<TData>> SendRequestGetResult<TData>(
            string route, 
            HttpMethod method, 
            object objectToBeSent = null, 
            (string key, object value)[] parameters = default,
            bool deserializeResponse = true
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
                return RequestResult<TData>.Failure(failureMessage: $"{nameof(ArgumentNullException)} was thrown by {nameof(HttpClient)}");
            }
            catch (InvalidOperationException)
            {
                return RequestResult<TData>.Failure(failureMessage: $"{nameof(InvalidOperationException)} was thrown by {nameof(HttpClient)}");
            }
            catch (HttpRequestException)
            {
                return RequestResult<TData>.Failure(failureMessage: $"{nameof(HttpRequestException)} was thrown by {nameof(HttpClient)}");
            }
            catch (TaskCanceledException)
            {
                return RequestResult<TData>.Failure(failureMessage: $"Http request timed out with timeout {_timeoutSeconds}s!");
            }

            if (httpResponse.IsSuccessStatusCode == false)
                return RequestResult<TData>.Failure(
                    failureMessage: $"Received failure http status code: {httpResponse?.StatusCode} {httpResponse?.ReasonPhrase}",
                    responseCode: httpResponse.StatusCode);

            var requestResultString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (deserializeResponse == false)
                return RequestResult<TData>.Success(default, httpResponse.StatusCode, requestResultString);

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

            return RequestResult<TData>.Success(responseDto, httpResponse.StatusCode, requestResultString);
        }


        private async Task<HttpResponseMessage> SendRequestGetHttpResponse(string route, HttpMethod method, string objectToBeSent = default, (string key, object value)[] parameters = default)
        {
            using (var content = new StringContent(objectToBeSent, Encoding.UTF8, "application/xml"))
            {
                var address = CombineAddress(route);
                address = AddParametersToUrl(address, parameters);

                using (var request = new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri(address)
                })
                {
                    if (method != HttpMethod.Get)
                        request.Content = content;

                    return await _httpClient.SendAsync(request).ConfigureAwait(false);
                }
            }
        }

        private string AddParametersToUrl(string address, (string key, object value)[] parameters)
        {
            if (parameters != default && parameters.Length > 0)
            {
                var parametersStr = string.Join("&",
                    parameters.Select(
                        kvp => $"{kvp.key}={kvp.value}")
                );

                address = Flurl.Url.Combine(address, "?", parametersStr);
            }

            return address;
        }

        private string CombineAddress(string route)
        {
            string https = string.Empty;
            if (_useSsl)
                https = "s";

            return Flurl.Url.Combine($"http{https}://{_apiHost}/", route);
        }
    }
}
