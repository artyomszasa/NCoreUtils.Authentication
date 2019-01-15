using System;

namespace NCoreUtils.Authentication.Linked
{
    class LoginDescriptor
    {
        public string Name { get; }

        public Type Type { get; }

        public object[] Arguments { get; }

        public LoginDescriptor(string name, Type type, params object[] arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Arguments = arguments;
        }
    }
}