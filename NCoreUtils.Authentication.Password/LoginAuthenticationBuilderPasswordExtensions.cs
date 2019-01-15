using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NCoreUtils.Authentication
{
    public static class LoginAuthenticationBuilderPasswordExtensions
    {
        public static LoginAuthenticationBuilder AddPasswordAuthentication<TUserManager, TUserId, TPasswordLogin>(this LoginAuthenticationBuilder builder, Action<PasswordLoginOptionsBuilder> configure = null)
            where TUserManager : class, IUserManager<TUserId>
            where TPasswordLogin : PasswordLogin<TUserId>
        {
            var optionsBuilder = new PasswordLoginOptionsBuilder();
            configure?.Invoke(optionsBuilder);
            builder
                .AddLogin<TPasswordLogin>()
                .Services
                    .AddSingleton(optionsBuilder.Build())
                    .TryAddTransient<IUserManager<TUserId>, TUserManager>();
            return builder;
        }

        public static LoginAuthenticationBuilder AddPasswordAuthentication<TUserManager, TUserId>(this LoginAuthenticationBuilder builder, Action<PasswordLoginOptionsBuilder> configure = null)
            where TUserManager : class, IUserManager<TUserId>
        {
            return builder.AddPasswordAuthentication<TUserManager, TUserId, PasswordLogin<TUserId>>(configure);
        }


        public static LoginAuthenticationBuilder AddPasswordAuthentication<TUserManager, TUserId>(this LoginAuthenticationBuilder builder, IConfiguration configuration)
            where TUserManager : class, IUserManager<TUserId>
            => builder.AddPasswordAuthentication<TUserManager, TUserId>(optionsBuilder => configuration.Bind(optionsBuilder));
    }
}