namespace Oddin.OddinSdk.SDK.Sessions.Abstractions
{
    internal interface IOpenable
    {
        /// <summary>
        /// Gets a value indicating whether the instance is opened
        /// </summary>
        /// <returns></returns>
        bool IsOpened();

        /// <summary>
        /// Opens the instance
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the the instance
        /// </summary>
        void Close();
    }
}
