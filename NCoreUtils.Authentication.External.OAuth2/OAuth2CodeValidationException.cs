using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Authentication.OAuth2
{
    [Serializable]
    public class OAuth2CodeValidationException : OAuth2RequestException
    {
        const string DefaultMessage = "OAuth2 code validation request failed.";

        const string KeyCode = "OAuth2" + nameof(Code);

        public string Code { get; }

        public OAuth2CodeValidationException(Uri endpoint, string code, string message)
            : base(endpoint, message)
            => Code = code;

        public OAuth2CodeValidationException(Uri endpoint, string code, string message, Exception innerException)
            : base(endpoint, message, innerException)
            => Code = code;

        public OAuth2CodeValidationException(Uri endpoint, string code)
            : this(endpoint, code, DefaultMessage)
        { }

        public OAuth2CodeValidationException(Uri endpoint, string code, Exception innerException)
            : this(endpoint, code, DefaultMessage, innerException)
        { }

        protected OAuth2CodeValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => Code = info.GetString(KeyCode);

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(KeyCode, Code);
        }
    }
}