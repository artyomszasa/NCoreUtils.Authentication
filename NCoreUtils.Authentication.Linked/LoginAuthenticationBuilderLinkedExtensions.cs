using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Authentication.Linked;

namespace NCoreUtils.Authentication
{
    public static class LoginAuthenticationBuilderLinkedExtensions
    {
        public static LoginAuthenticationBuilder AddLinkedLogin<TUserManager, TLinkedUserManager, TId>(
            this LoginAuthenticationBuilder builder,
            Action<LoginAuthenticationBuilder> externalLoginConfigure)
            where TUserManager : class, IUserManager<TId>
            where TLinkedUserManager : class, ILinkedUserManager
        {
            var externalLoginBuilder = new LoginAuthenticationBuilder(builder.Services);
            externalLoginConfigure(externalLoginBuilder);
            foreach (var kv in externalLoginBuilder)
            {
                var desc = new LoginDescriptor(kv.Key, kv.Value.Type, kv.Value.args);
                builder.AddLogin<LinkedLogin<TId>>(desc.Name, desc);
            }
            builder.Services.TryAddTransient<IUserManager<TId>, TUserManager>();
            builder.Services.TryAddTransient<ILinkedUserManager, TLinkedUserManager>();
            return builder;
        }

        public static LoginAuthenticationBuilder AddLinkedLogin<TUserManager, TLinkedUserManager>(
            this LoginAuthenticationBuilder builder,
            Action<LoginAuthenticationBuilder> externalLoginConfigure)
            where TUserManager : class, IUserManager<int>
            where TLinkedUserManager : class, ILinkedUserManager
            => builder.AddLinkedLogin<TUserManager, TLinkedUserManager, int>(externalLoginConfigure);

        public static LoginAuthenticationBuilder AddLinkedLogin<TUserManager>(
            this LoginAuthenticationBuilder builder,
            Action<LoginAuthenticationBuilder> externalLoginConfigure)
            where TUserManager : class, IUserManager<int>, ILinkedUserManager
            => builder.AddLinkedLogin<TUserManager, TUserManager>(externalLoginConfigure);

    }
}