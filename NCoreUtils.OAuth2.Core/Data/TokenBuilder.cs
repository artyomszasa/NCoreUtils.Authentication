using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NCoreUtils.OAuth2.Data
{
    public class TokenBuilder
    {
        public string Id { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset IssuedAt { get; set; }

        public HashSet<string> Scopes { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TokenBuilder(string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, HashSet<string> scopes)
        {
            Id = id;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            Scopes = scopes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TokenBuilder() : this(null, DateTimeOffset.MinValue, DateTimeOffset.MinValue, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TokenBuilder(Token token) : this(token.Id, token.IssuedAt, token.ExpiresAt, new HashSet<string>(token.Scopes, StringComparer.OrdinalIgnoreCase)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token Build() => new Token(this);
    }
}