using System;

namespace Oddin.OddinSdk.SDK.Dispatch.EventArguments
{
    /// <summary>
    /// Event arguments for the FeedClose events
    /// </summary>
    public class FeedCloseEventArgs : EventArgs
    {
        private readonly string _reason;

        internal FeedCloseEventArgs(string reason)
        {
            if (reason is null)
                throw new ArgumentNullException(nameof(reason));

            if (reason == string.Empty)
                throw new ArgumentException($"Argument {nameof(reason)} cannot be empty!");

            _reason = reason;
        }

        /// <summary>
        /// Gets a <see cref="string"/> containing the reason why feed must be closed
        /// </summary>
        /// <returns>Returns a <see cref="string"/> containing the reason why feed must be closed</returns>
        public string GetReason()
        {
            return _reason;
        }
    }
}
