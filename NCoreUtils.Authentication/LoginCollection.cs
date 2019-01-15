using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NCoreUtils.Authentication
{
    public class LoginCollection : IReadOnlyDictionary<string, (Type type, object[] args)>
    {
        readonly ImmutableDictionary<string, (Type type, object[] args)> _loginTypes;

        public (Type type, object[] args) this[string key] => _loginTypes[key];

        public IEnumerable<string> Keys => _loginTypes.Keys;

        public IEnumerable<(Type type, object[] args)> Values => _loginTypes.Values;

        public int Count => _loginTypes.Count;

        public LoginCollection(ImmutableDictionary<string, (Type type, object[] args)> loginTypes)
            => _loginTypes = loginTypes ?? throw new ArgumentNullException(nameof(loginTypes));

        public bool ContainsKey(string key) => _loginTypes.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, (Type type, object[] args)>> GetEnumerator() => _loginTypes.GetEnumerator();

        public bool TryGetValue(string key, out (Type type, object[] args) value) => _loginTypes.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}