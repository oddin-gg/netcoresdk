using System.Net;

namespace Oddin.OddinSdk.SDK.API.Abstractions
{
    public interface IRequestResult<TData>
        where TData : class
    {
        /// <summary>
        /// Deserialized content of response to the request
        /// </summary>
        TData Data { get; }

        /// <summary>
        /// <see cref="true"/> if the request was successful
        /// </summary>
        bool Successful { get; }

        /// <summary>
        /// Details of request success of failure
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Response code of original HTTP request
        /// </summary>
        HttpStatusCode ResponseCode { get; }

        /// <summary>
        /// <see cref="string"/> representation of the data that was received (before deserialization)
        /// </summary>
        string RawData { get; }
    }
}
