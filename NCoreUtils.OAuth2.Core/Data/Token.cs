using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NCoreUtils.OAuth2.Data
{
    public class Token : IEquatable<Token>
    {
        const short Magic = 0x7EE7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static (string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, ImmutableHashSet<string> scopes) ReadFrom(BinaryReader reader)
        {
            var magic = reader.ReadInt16();
            if (Magic != magic)
            {
                throw new FormatException("Invalid magic bytes in token.");
            }
            var id = reader.ReadString();
            var issuedAtTicks = reader.ReadInt64();
            var expiresAtTicks = reader.ReadInt64();
            var scopeCount = reader.ReadInt32();
            var scopes = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < scopeCount; ++i)
            {
                scopes.Add(reader.ReadString());
            }
            return (id, new DateTimeOffset(issuedAtTicks, TimeSpan.Zero), new DateTimeOffset(expiresAtTicks, TimeSpan.Zero), scopes.ToImmutable());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ImmutableHashSet<string> SafeToArray(IEnumerable<string> scopes)
        {
            return null == scopes ? ImmutableHashSet<string>.Empty : scopes.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ImmutableHashSet<string> SafeToArray(string[] scopes)
        {
            return null == scopes ? ImmutableHashSet<string>.Empty : scopes.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator== (Token a, Token b) => ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!= (Token a, Token b) => ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

        public string Id { get; }

        public DateTimeOffset IssuedAt { get; }

        public DateTimeOffset ExpiresAt { get; }

        public ImmutableHashSet<string> Scopes { get; }

        Token((string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, ImmutableHashSet<string> scopes) data)
            : this(data.id, data.issuedAt, data.expiresAt, data.scopes)
        { }

        public Token(string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, ImmutableHashSet<string> scopes)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            if (null == scopes)
            {
                Scopes = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
            }
            else if (ReferenceEquals(scopes.KeyComparer, StringComparer.OrdinalIgnoreCase))
            {
                Scopes = scopes;
            }
            else
            {
                Scopes = scopes.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token(string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, IEnumerable<string> scopes)
            : this(id, issuedAt, expiresAt, SafeToArray(scopes))
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token(string id, DateTimeOffset issuedAt, DateTimeOffset expiresAt, params string[] scopes)
            : this(id, issuedAt, expiresAt, SafeToArray(scopes))
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token(TokenBuilder builder)
            : this(builder.Id, builder.IssuedAt, builder.ExpiresAt, builder.Scopes)
        { }

        public Token(BinaryReader reader)
            : this(ReadFrom(reader))
        { }

        public override bool Equals(object obj) => Equals(obj as Token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Token other)
        {
            if (null == other)
            {
                return false;
            }
            return StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id)
                && IssuedAt.Equals(other.IssuedAt)
                && ExpiresAt.Equals(other.ExpiresAt)
                && Scopes.SetEquals(other.Scopes);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
                hash = hash * 23 + IssuedAt.GetHashCode();
                hash = hash * 23 + ExpiresAt.GetHashCode();
                foreach (var scope in Scopes)
                {
                    hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(scope);
                }
                return hash;
            }
        }


        public Token Update(Action<TokenBuilder> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            var builder = new TokenBuilder(this);
            update(builder);
            return builder.Build();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Id);
            writer.Write(IssuedAt.UtcTicks);
            writer.Write(ExpiresAt.UtcTicks);
            writer.Write(Scopes.Count);
            foreach (var scope in Scopes)
            {
                writer.Write(scope);
            }
        }
    }
}