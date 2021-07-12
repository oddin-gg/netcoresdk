using System;

namespace Oddin.OddinSdk.Common.Exceptions
{
    public class MappingException : Exception
    {
        public MappingException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
