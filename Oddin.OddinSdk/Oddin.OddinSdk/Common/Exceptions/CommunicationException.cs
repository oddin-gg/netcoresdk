using System;
using System.Net;

namespace Oddin.OddinSdk.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK when an error occurred while communicating with external source (Feed REST-ful API)
    /// </summary>
    /// <seealso cref="FeedSdkException" />
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
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CommunicationException(string message) : base(message)
        {

        }
    }
}
