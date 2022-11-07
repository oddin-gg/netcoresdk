using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oddin.OddsFeedSdk.API.Abstractions;

internal interface IRestClient
{
    public Task<RequestResult<TData>> SendRequestAsync<TData>(
        string route,
        HttpMethod method,
        CultureInfo culture = null,
        object objectToBeSent = null,
        (string key, object value)[] parameters = default,
        bool deserializeResponse = true,
        bool ignoreUnsuccessfulStatusCode = false
    )
        where TData : class;

    public RequestResult<TData> SendRequest<TData>(
        string route,
        HttpMethod method,
        CultureInfo culture = null,
        object objectToBeSent = null,
        (string key, object value)[] parameters = default,
        bool deserializeResponse = true,
        bool ignoreUnsuccessfulStatusCode = false
    )
        where TData : class;

    IObservable<T> SubscribeForClass<T>();
}