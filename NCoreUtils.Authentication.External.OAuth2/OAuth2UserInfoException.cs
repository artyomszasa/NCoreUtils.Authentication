using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Authentication.OAuth2
{
    [Serializable]
    public class OAuth2UserInfoException : OAuth2RequestException
    {
        const string DefaultMessage = "OAuth2 user info request failed.";

        const string KeyAccessToken = "OAuth2" + nameof(AccessToken);

        public string AccessToken { get; }

        public OAuth2UserInfoException(Uri endpoint, string accessToken, string message)
            : base(endpoint, message)
            => AccessToken = accessToken;

        public OAuth2UserInfoException(Uri endpoint, string accessToken, string message, Exception innerException)
            : base(endpoint, message, innerException)
            => AccessToken = accessToken;

        public OAuth2UserInfoException(Uri endpoint, string accessToken)
            : this(endpoint, accessToken, DefaultMessage)
        { }

        public OAuth2UserInfoException(Uri endpoint, string accessToken, Exception innerException)
            : this(endpoint, accessToken, DefaultMessage, innerException)
        { }

        protected OAuth2UserInfoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => AccessToken = info.GetString(KeyAccessToken);

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(KeyAccessToken, AccessToken);
        }
    }
}