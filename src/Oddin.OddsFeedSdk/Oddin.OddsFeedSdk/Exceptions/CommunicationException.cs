using System;
using System.Net;

namespace Oddin.OddsFeedSdk.Exceptions;

/// <summary>
///     Exception when communicating with API
/// </summary>
public class CommunicationException : SdkException
{
    public readonly string Response;

    public readonly HttpStatusCode ResponseCode;

    public CommunicationException(string message)
        : base(message) =>
        Url = null;

    public CommunicationException(string message, string url, Exception innerException)
        : base(message, innerException) =>
        Url = url;

    public CommunicationException(string message, string url, HttpStatusCode responseCode, Exception innerException)
        : base(message, innerException)
    {
        Url = url;
        ResponseCode = responseCode;
    }

    public CommunicationException(string message, string url, HttpStatusCode responseCode, string response,
        Exception innerException)
        : base(message, innerException)
    {
        Url = url;
        ResponseCode = responseCode;
        Response = response;
    }

    public string Url { get; }

    public override string ToString()
        => $"Url={Url}|ResponseCode={ResponseCode}|Response={Response}";
}