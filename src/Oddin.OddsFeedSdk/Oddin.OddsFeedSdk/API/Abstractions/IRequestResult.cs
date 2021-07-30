using System.Globalization;
using System.Net;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    public interface IRequestResult<out TData>
        where TData : class
    {
        TData Data { get; }

        bool Successful { get; }

        string Message { get; }

        HttpStatusCode ResponseCode { get; }

        string RawData { get; }

        CultureInfo Culture { get; }
    }
}
