using Microsoft.Extensions.Logging;
using Oddin.Oddin.Common;
using Oddin.Oddin.DTOs.API.Entities;
using Oddin.Oddin.SDK.API.Entities;
using Oddin.Oddin.SDK.Managers;
using Oddin.OddinSdk.SDK;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Oddin.Oddin.SDK.API
{
    internal class ApiClient : LoggingBase, IApiClient
    {
        private readonly string _apiHost;
        private readonly bool _useSsl;
        private readonly int _timeoutSeconds;
        private HttpClient _httpClient = new HttpClient();

        public ApiClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _apiHost = config.ApiHost;
            _useSsl = config.UseApiSsl;
            _timeoutSeconds = config.HttpClientTimeout;

            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("x-access-token", config.AccessToken);
        }

        public List<IProducer> GetProducers()
        {
            var response = SendRequest<ProducersDto>("v1/descriptions/producers", HttpMethod.Get);

            // TODO: generalize DTO to entity translation
            var result = new List<IProducer>();
            foreach (var producer in response.Data.producer)
                result.Add(new Producer(
                    producer.id,
                    producer.name,
                    producer.description,
                    producer.active,
                    producer.scope,
                    producer.stateful_recovery_window_in_minutes));


            if (response.Successful == false)
            {
                // TODO: propagate failure somehow ???
            }

            return result;
        }


        // TODO: look for HttpClient.Send(), should be available for .net standard 2.1
        private RequestResult<TData> SendRequest<TData>(string route, HttpMethod method, object objectToBeSent = null)
            where TData : class
        {
            var task = Task.Run(
                () => SendRequestAsync<TData>(route, method, objectToBeSent));

            task.Wait();
            if (task.Result.Successful == false)
                _log.LogError($"Http request [{method} to {CombineAddress(route)}] failed. Reason: {task.Result.Message}");

            return task.Result;
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
                return RequestResult<TData>.Failure(failureMessage: $"Received failure http status code: {httpResponse?.StatusCode} {httpResponse?.ReasonPhrase}");

            var requestResultString = await httpResponse.Content.ReadAsStringAsync();

            if (XmlHelper.TryDeserialize<TData>(requestResultString, out var responseDto) == false)
                return RequestResult<TData>.Failure(failureMessage: $"Unable to deserialize response! Response:\n{requestResultString}");

            if (responseDto is null)
                return RequestResult<TData>.Failure(failureMessage: "Response was deserialized as null!");

            return RequestResult<TData>.Success(responseDto);
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

                    return await _httpClient.SendAsync(request);
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

    public interface IApiClient
    {
        /// <summary>
        /// Gets a list of <see cref="IProducer"/> from API
        /// </summary>
        /// <returns>The list of <see cref="IProducer"/></returns>
        List<IProducer> GetProducers();
    }
}
