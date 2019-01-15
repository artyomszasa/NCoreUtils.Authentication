using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Authentication.OAuth2;

namespace NCoreUtils.Authentication
{
    public static class LoginAuthenticationBuilderGoogleExtensions
    {
        public static LoginAuthenticationBuilder AddGoogleLogin(this LoginAuthenticationBuilder builder, Action<OAuth2ConfigurationBuilder> configure)
        {
            var configurationBuilder = new OAuth2ConfigurationBuilder();
            configure(configurationBuilder);
            builder
                .AddLogin<GoogleLogin>()
                .Services
                    .AddSingleton(configurationBuilder.Build<GoogleLogin>());
            return builder;
        }

        public static LoginAuthenticationBuilder AddGoogleLogin(this LoginAuthenticationBuilder builder, IConfiguration configuration)
            => builder.AddGoogleLogin(configurationBuilder => configuration.Bind(configurationBuilder));
    }
}