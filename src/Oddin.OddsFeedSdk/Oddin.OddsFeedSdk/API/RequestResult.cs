using System;
using System.Globalization;
using System.Net;
using Oddin.OddsFeedSdk.API.Abstractions;

namespace Oddin.OddsFeedSdk.API;

internal class RequestResult<TData> : IRequestResult<TData>
    where TData : class
{
    private readonly TData _data;

    public TData Data
    {
        get => Successful ? _data : throw new InvalidOperationException("Unable to get data from failed result");
        init => _data = value;
    }

    public bool Successful { get; init; }

    public string Message { get; init; }

    public HttpStatusCode ResponseCode { get; init; }

    public string RawData { get; init; }

    public CultureInfo Culture { get; init; }

    public static RequestResult<TData> Success(TData data, HttpStatusCode responseCode, string rawData,
        string successMessage = "", CultureInfo culture = default)
        => new()
        {
            Data = data,
            Successful = true,
            Message = successMessage,
            ResponseCode = responseCode,
            RawData = rawData,
            Culture = culture
        };

    public static RequestResult<TData> Failure(HttpStatusCode responseCode = default, string rawData = "",
        string failureMessage = "")
        => new()
        {
            Successful = false,
            Message = failureMessage,
            ResponseCode = responseCode,
            RawData = rawData
        };
}