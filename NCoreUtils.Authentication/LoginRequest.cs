using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Authentication
{
    public struct LoginRequest : IEquatable<LoginRequest>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool operator==(LoginRequest a, LoginRequest b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool operator!=(LoginRequest a, LoginRequest b) => !a.Equals(b);

        readonly string _loginName;

        readonly string _passcode;

        public string LoginName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerStepThrough]
            get => _loginName;
        }

        public string Passcode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [DebuggerStepThrough]
            get => _passcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public LoginRequest(string loginName, string passcode)
        {
            _loginName = loginName;
            _passcode = passcode;
        }

        [DebuggerStepThrough]
        public override bool Equals(object obj) => obj is LoginRequest other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public bool Equals(LoginRequest other) => StringComparer.OrdinalIgnoreCase.Equals(LoginName, other.LoginName) && Passcode == other.Passcode;

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + (LoginName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(LoginName));
            hash = hash * 23 + (Passcode?.GetHashCode() ?? 0);
            return hash;
        }

    }
}