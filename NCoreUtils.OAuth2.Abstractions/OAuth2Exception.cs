using System;
using System.Runtime.Serialization;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class OAuth2Exception : Exception
    {
        const string BaseMessage = "OAuth2 exception has been thrown.";

        public OAuth2Error Error { get; }

        public string ErrorDescription { get; }

        public override string Message => $"({Error}) {ErrorDescription ?? base.Message}";

        protected OAuth2Exception(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Error = (OAuth2Error)info.GetInt32(nameof(OAuth2Error));
            ErrorDescription = info.GetString(nameof(ErrorDescription));
        }

        public OAuth2Exception(OAuth2Error error, string errorDescription = null, Exception innerException = null)
            : base(BaseMessage, innerException)
        {
            Error = error;
            ErrorDescription = errorDescription;
        }

        public OAuth2Exception(OAuth2Error error, Exception innerException)
            : this(error, null, innerException)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(OAuth2Error), (int)Error);
            info.AddValue(nameof(ErrorDescription), ErrorDescription);
        }
    }
}