using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Authentication
{
    [Serializable]
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message)
            : base(message)
        { }

        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected AuthenticationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}