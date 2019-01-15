using System;
using Microsoft.Extensions.Configuration;

namespace NCoreUtils.Authentication.OAuth2
{
    public class OAuth2ConfigurationBuilder
    {
        public OAuth2EndPointConfigurationBuilder AccessTokenEndPoint { get; set; } = new OAuth2EndPointConfigurationBuilder();

        public OAuth2EndPointConfigurationBuilder UserInfoEndPoint { get; set; } = new OAuth2EndPointConfigurationBuilder();

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public OAuth2ConfigurationBuilder ConfigureAccessTokenEndpoint(Action<OAuth2EndPointConfigurationBuilder> configure)
        {
            configure(AccessTokenEndPoint);
            return this;
        }

        public OAuth2ConfigurationBuilder ConfigureAccessTokenEndpoint(IConfiguration configuration)
        {
            configuration.Bind(AccessTokenEndPoint);
            return this;
        }

        public OAuth2ConfigurationBuilder ConfigureUserInfoEndpoint(Action<OAuth2EndPointConfigurationBuilder> configure)
        {
            configure(UserInfoEndPoint);
            return this;
        }

        public OAuth2ConfigurationBuilder ConfigureUserInfoEndpoint(IConfiguration configuration)
        {
            configuration.Bind(UserInfoEndPoint);
            return this;
        }

        public OAuth2ConfigurationBuilder WithClientId(string clientId)
        {
            ClientId = clientId;
            return this;
        }

        public OAuth2ConfigurationBuilder WithClientSecret(string clientSecret)
        {
            ClientSecret = clientSecret;
            return this;
        }

        public OAuth2ConfigurationBuilder WithRedirectUri(string redirectUri)
        {
            RedirectUri = redirectUri;
            return this;
        }

        public virtual OAuth2Configuration Build() => new OAuth2Configuration(this);

        public virtual OAuth2Configuration<TLogin> Build<TLogin>() => new OAuth2Configuration<TLogin>(this);
    }
}