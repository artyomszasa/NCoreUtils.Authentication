using System;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Authentication
{
    public static class PasswordLogin
    {
        static readonly Random _random = new Random((int)(DateTime.Now.Ticks % ((long)int.MaxValue + 1L)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPasswordHash(string password, string salt)
            => Sha512Helper.CalculateHash($"{password}:{salt}");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string Hash, string Salt) GeneratePaswordHash(string password)
        {
            var salt = Sha512Helper.CalculateHash(_random.Next().ToString());
            return (GetPasswordHash(password, salt), salt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckPasswordHash(string hash, string salt, string password)
            => StringComparer.OrdinalIgnoreCase.Equals(GetPasswordHash(password, salt), hash);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckPasswordHash<TUserId>(ILocalUser<TUserId> user, string password)
            => CheckPasswordHash(user.Password, user.Salt, password);
    }
}