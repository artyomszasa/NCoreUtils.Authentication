using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Authentication.OAuth2;
using NCoreUtils.Authentication.OpenId;

namespace NCoreUtils.Authentication
{
    public static class OpenIdLoginAuthenticationBuilderExtensions
    {
        public static LoginAuthenticationBuilder AddOpenIdLogin(this LoginAuthenticationBuilder builder, Action<OAuth2ConfigurationBuilder> configure)
        {
            var configurationBuilder = new OAuth2ConfigurationBuilder();
            configure(configurationBuilder);
            builder
                .AddLogin<OpenIdLogin>()
                .Services
                    .AddSingleton(configurationBuilder.Build<OpenIdLogin>());
            return builder;
        }

        public static LoginAuthenticationBuilder AddOpenIdLogin(this LoginAuthenticationBuilder builder, IConfiguration configuration)
            => builder.AddOpenIdLogin(configurationBuilder => configuration.Bind(configurationBuilder));

    }
}