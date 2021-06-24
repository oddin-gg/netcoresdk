using System;
using System.Net;

namespace Oddin.OddinSdk.SDK.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK when an error occurred while communicating with external source (Feed REST-ful API)
    /// </summary>
    public class CommunicationException : Exception
    {
        /// <summary>
        /// Gets the <see cref="string"/> representation of the url specifying the resource which was being accessed
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> specifying the response's status code
        /// </summary>
        public readonly HttpStatusCode ResponseCode;

        /// <summary>
        /// Gets the <see cref="string"/> representation of the response received from the external source
        /// </summary>
        public readonly string Response;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class
        /// </summary>
        /// <param name="message">The error message explaining the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified</param>
        /// <param name="url">The <see cref="string"/> representation of the url specifying the resource which was being accessed</param>
        /// <param name="responseCode">The <see cref="HttpStatusCode"/> specifying the response's status code</param>
        /// <param name="response">The <see cref="string"/> representation of the response received from the external source</param>
        public CommunicationException(string message, Exception innerException, string url, HttpStatusCode responseCode, string response) : base(message, innerException)
        {
            Url = url;
            ResponseCode = responseCode;
            Response = response;
        }
    }
}
