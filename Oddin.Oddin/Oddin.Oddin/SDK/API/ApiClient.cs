using Microsoft.Extensions.Logging;
using Oddin.Oddin.SDK.API.Entities;
using Oddin.Oddin.SDK.Managers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oddin.Oddin.SDK.API
{
    internal class ApiClient : LoggingBase, IApiClient
    {
        // TODO: move to a configuration file
        public const string API_KEY = "1a0c5a30-74ed-416d-b120-8c05f92e382f";

        private HttpClient _httpClient = new HttpClient();

        public ApiClient(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _httpClient.DefaultRequestHeaders.Add("x-access-token", API_KEY);
        }

        public IRequestResult<List<IProducer>> GetProducers()
            => SendRequest<List<IProducer>>("https://api-mq.integration.oddin.gg/v1/descriptions/producers", HttpMethod.Get);

        private RequestResult<TData> SendRequest<TData>(string route, HttpMethod method, object objectToBeSent = null)
            where TData : class
        {
            // xml serialization
            HttpResponseMessage httpResponse;
            // try
            httpResponse = SendRequestGetResponse(route, method, objectToBeSent);
            // catch

            if (httpResponse.IsSuccessStatusCode == false)
                return RequestResult<TData>.Failure(failureMessage: $"Received failure http status code: {httpResponse?.StatusCode} {httpResponse?.ReasonPhrase}");

            var requestResultString = httpResponse.Content.ReadAsStringAsync();
            // xml deserialization

            // create requestResult based on deserialization success
            //RequestResult<TData>.Success(...);
            //RequestResult<TData>.Failure(...);
            return null;
        }

        private HttpResponseMessage SendRequestGetResponse(string route, HttpMethod method, object objectToBeSent = null)
        {
            using (var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(route)
            })
            {
                // TODO: look for HttpClient.Send(), should be available for .net standard 2.1
                var task = Task.Run(() => _httpClient.SendAsync(request));
                task.Wait();
                return task.Result;
            }
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
