using Oddin.OddinSdk.SDK.API.Abstractions;
using System.Net;

namespace Oddin.OddinSdk.SDK.API
{
    internal class RequestResult<TData> : IRequestResult<TData>
        where TData : class
    {
        public TData Data { get; set; }

        public bool Successful { get; set; }

        public string Message { get; set; }

        public HttpStatusCode ResponseCode { get; set; }

        public string RawData { get; set; }

        public static RequestResult<TData> Success(TData data, HttpStatusCode responseCode, string rawData, string successMessage = "")
            => new RequestResult<TData>()
            {
                Data = data,
                Successful = true,
                Message = successMessage,
                ResponseCode = responseCode,
                RawData = rawData
            };

        public static RequestResult<TData> Failure(HttpStatusCode responseCode = default, string rawData = "", string failureMessage = "", TData data = default)
            => new RequestResult<TData>()
            {
                Data = data,
                Successful = false,
                Message = failureMessage,
                ResponseCode = responseCode,
                RawData = rawData
            };
    }
}
