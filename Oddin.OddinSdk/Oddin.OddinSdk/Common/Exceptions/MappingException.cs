using System;

namespace Oddin.OddinSdk.Common.Exceptions
{
    public class MappingException : SdkException
    {
        public MappingException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
