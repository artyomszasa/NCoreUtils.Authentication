using System;

namespace NCoreUtils.Authentication.OAuth2
{
    public sealed class OAuth2EndPointConfiguration
    {
        public string Uri { get; }

        public OAuth2RequestMethod Method { get; }

        public OAuth2EndPointConfiguration(OAuth2EndPointConfigurationBuilder builder)
        {
            Uri = builder.Uri ?? throw new ArgumentNullException($"{nameof(builder)}.{nameof(builder.Uri)}");
            Method = builder.Method;
        }
    }
}