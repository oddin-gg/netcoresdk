namespace Oddin.OddinSdk.SDK.FeedConfiguration
{
    public enum ExceptionHandlingStrategy
    {
        /// <summary>
        /// Specifies a strategy in which the exceptions are thrown to caller
        /// </summary>
        THROW = 0,

        /// <summary>
        /// Specifies a strategy in which all exceptions are handled by the called instance
        /// </summary>
        CATCH = 1
    }
}
