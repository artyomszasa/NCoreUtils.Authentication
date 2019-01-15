using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Authentication
{
    public class LoginAuthenticationBuilder : IEnumerable<KeyValuePair<string, (Type Type, object[] args)>>
    {
        internal readonly ConcurrentDictionary<string, (Type type, object[] args)> _loginTypes = new ConcurrentDictionary<string, (Type type, object[] args)>();

        public IServiceCollection Services { get; }

        public LoginAuthenticationBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public LoginAuthenticationBuilder AddLogin<TLogin>(params object[] args)
            where TLogin : ILogin
        {
            var attr = typeof(TLogin).GetCustomAttribute<LoginAttribute>();
            if (null == attr)
            {
                throw new InvalidOperationException($"Specified type {typeof(TLogin).FullName} has no LoginAttribute. Each login logic must define locally unique name through LoginAttribute.");
            }
            return AddLogin<TLogin>(attr.Name, args);
        }

        public LoginAuthenticationBuilder AddLogin<TLogin>(string name, params object[] args)
        {
            if (!_loginTypes.TryAdd(name, (typeof(TLogin), args)))
            {
                throw new InvalidOperationException($"Login with name {name} has already been registered.");
            }
            return this;
        }

        public IEnumerator<KeyValuePair<string, (Type Type, object[] args)>> GetEnumerator() => _loginTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class LoginAuthenticationBuilder<TConfiguration> : LoginAuthenticationBuilder
    {
        public LoginAuthenticationBuilder(IServiceCollection services) : base(services) { }
    }
}