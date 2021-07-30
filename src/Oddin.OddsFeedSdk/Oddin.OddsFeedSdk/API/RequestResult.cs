using Oddin.OddsFeedSdk.API.Abstractions;
using System;
using System.Net;

namespace Oddin.OddsFeedSdk.API
{
    internal class RequestResult<TData> : IRequestResult<TData>
        where TData : class
    {
        private TData _data;
        public TData Data
        {
            get => Successful ? _data : throw new InvalidOperationException("Unable to get data from failed result");
            init => _data = Data;
        }

        public bool Successful { get; init; }

        public string Message { get; init; }

        public HttpStatusCode ResponseCode { get; init; }

        public string RawData { get; init; }

        public static RequestResult<TData> Success(TData data, HttpStatusCode responseCode, string rawData, string successMessage = "")
            => new RequestResult<TData>()
            {
                Data = data,
                Successful = true,
                Message = successMessage,
                ResponseCode = responseCode,
                RawData = rawData
            };

        public static RequestResult<TData> Failure(HttpStatusCode responseCode = default, string rawData = "", string failureMessage = "")
            => new RequestResult<TData>()
            {
                Successful = false,
                Message = failureMessage,
                ResponseCode = responseCode,
                RawData = rawData
            };
    }
}
