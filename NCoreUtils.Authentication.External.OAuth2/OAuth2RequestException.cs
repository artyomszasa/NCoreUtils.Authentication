using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Authentication.OAuth2
{
    [Serializable]
    public class OAuth2RequestException : AuthenticationException
    {
        const string DefaultMessage = "OAuth2 request failed.";

        const string KeyEndPoint = "OAuth2" + nameof(EndPoint);

        public Uri EndPoint { get; }

        public OAuth2RequestException(Uri endpoint, string message)
            : base(message)
            => EndPoint = endpoint;

        public OAuth2RequestException(Uri endpoint, string message, Exception innerException)
            : base(message, innerException)
            => EndPoint = endpoint;

        public OAuth2RequestException(Uri endpoint)
            : this(endpoint, DefaultMessage)
        { }

        public OAuth2RequestException(Uri endpoint, Exception innerException)
            : this(endpoint, DefaultMessage, innerException)
        { }

        public OAuth2RequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => EndPoint = new Uri(info.GetString(KeyEndPoint));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(KeyEndPoint, EndPoint.ToString());
        }
    }
}