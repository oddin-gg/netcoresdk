using Microsoft.Extensions.Logging;
using Oddin.Oddin.Common;
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
        // TODO: use
        private readonly int _timeoutSeconds;

        private HttpClient _httpClient = new HttpClient();

        public ApiClient(IOddsFeedConfiguration config, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _httpClient.DefaultRequestHeaders.Add("x-access-token", config.AccessToken);
        }

        public IRequestResult<List<IProducer>> GetProducers()
        {
            SendRequest<ProducersDTO>("v1/descriptions/producers", HttpMethod.Get);
            
            // TODO: convert DTO to entity
            return RequestResult<List<IProducer>>.Success(new List<IProducer>());
        }


        // TODO: look for HttpClient.Send(), should be available for .net standard 2.1
        private RequestResult<TData> SendRequest<TData>(string route, HttpMethod method, object objectToBeSent = null)
            where TData : class
        {
            var task = Task.Run(() => SendRequestAsync<TData>(route, method, objectToBeSent));
            task.Wait();
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

            if (httpResponse.IsSuccessStatusCode == false)
                return RequestResult<TData>.Failure(failureMessage: $"Received failure http status code: {httpResponse?.StatusCode} {httpResponse?.ReasonPhrase}");

            var requestResultString = await httpResponse.Content.ReadAsStringAsync();

            if (XmlHelper.TryDeserialize<TData>(requestResultString, out var responseDto) == false)
                return RequestResult<TData>.Failure(failureMessage: "Unable to deserialize response!");

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
        IRequestResult<List<IProducer>> GetProducers();
    }
}
