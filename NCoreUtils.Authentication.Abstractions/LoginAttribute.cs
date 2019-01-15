using System;

namespace NCoreUtils.Authentication
{
    public class LoginAttribute : Attribute
    {
        public string Name { get; }

        public LoginAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Login name must be non-empty string.", nameof(name));
            }
            Name = name;
        }
    }
}