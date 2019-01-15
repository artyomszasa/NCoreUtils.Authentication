using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Authentication
{
    public static class ServiceCollectionLoginAuthenticationExtensions
    {
        public static IServiceCollection AddLoginAuthentication<TAuthentication>(this IServiceCollection services, Action<LoginAuthenticationBuilder> init)
            where TAuthentication : class, ILoginAuthentication
        {
            var builder = new LoginAuthenticationBuilder(services);
            init(builder);
            return services
                .AddSingleton(new LoginCollection(builder._loginTypes.ToImmutableDictionary()))
                .AddScoped<ILoginAuthentication, TAuthentication>();
        }

        public static IServiceCollection AddLoginAuthentication(this IServiceCollection services, Action<LoginAuthenticationBuilder> init)
            => services.AddLoginAuthentication<LoginAuthentication>(init);

        public static IServiceCollection AddLoginAuthenticator<TAuthenticator>(this IServiceCollection services, Action<LoginAuthenticationBuilder> init)
            where TAuthenticator : LoginAuthenticator
        {
            var builder = new LoginAuthenticationBuilder(services);
            init(builder);
            return services
                .AddSingleton(new LoginCollection(builder._loginTypes.ToImmutableDictionary()))
                .AddScoped<LoginAuthenticator, TAuthenticator>();
        }

        public static IServiceCollection AddLoginAuthenticator(this IServiceCollection services, Action<LoginAuthenticationBuilder> init)
            => services.AddLoginAuthenticator<LoginAuthenticator>(init);
    }
}