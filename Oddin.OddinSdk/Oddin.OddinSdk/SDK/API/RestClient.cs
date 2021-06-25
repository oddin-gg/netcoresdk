using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.Common;
using Oddin.OddinSdk.SDK.Exceptions;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;
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

        public RestClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _apiHost = config.ApiHost;
            _useSsl = config.UseApiSsl;
            _timeoutSeconds = config.HttpClientTimeout;

            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("x-access-token", config.AccessToken);
        }


        public TData SendRequest<TData>(string route, HttpMethod method, object objectToBeSent = null)
            where TData : class
        {
            RequestResult<TData> result;
            try
            {
                result = SendRequestAsync<TData>(route, method, objectToBeSent).GetAwaiter().GetResult();
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

            return result.Data;
        }

        private async Task<RequestResult<TData>> SendRequestAsync<TData>(string route, HttpMethod method, object objectToBeSent = null)
            where TData : class
        {
            if (XmlHelper.TrySerialize(objectToBeSent, out var serializedObject) == false)
                return RequestResult<TData>.Failure(failureMessage: "Request could not be serialized!");

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await SendRequestGetResponseAsync(route, method, serializedObject);
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

            return RequestResult<TData>.Success(responseDto,httpResponse.StatusCode, requestResultString);
        }


        private async Task<HttpResponseMessage> SendRequestGetResponseAsync(string route, HttpMethod method, string objectToBeSent = default)
        {
            using (var content = new StringContent(objectToBeSent, Encoding.UTF8, "application/xml"))
            {
                using (var request = new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri(CombineAddress(route))
                })
                {
                    if (method != HttpMethod.Get)
                        request.Content = content;

                    return await _httpClient.SendAsync(request).ConfigureAwait(false);
                }
            }
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
