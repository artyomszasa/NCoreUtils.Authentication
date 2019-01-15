using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Authentication
{
    /// <summary>
    /// Compact POCO capable of storing claim information.
    /// </summary>
    [Serializable]
    public sealed class ClaimDescriptor : IEquatable<ClaimDescriptor>, ISerializable
    {
        public string Type { get; }

        public string Value { get; }

        public string ValueType { get; }

        public string Issuer { get; }

        public string OriginalIssuer { get; }

        internal ClaimDescriptor(SerializationInfo info, StreamingContext context)
            : this(info.GetString(nameof(Type)), info.GetString(nameof(Value)), info.GetString(nameof(ValueType)), info.GetString(nameof(Issuer)), info.GetString(nameof(OriginalIssuer)))
        { }

        public ClaimDescriptor(string type, string value, string valueType = null, string issuer = null, string originalIssuer = null)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
            ValueType = valueType;
            Issuer = issuer;
            OriginalIssuer = originalIssuer;
        }

        public ClaimDescriptor(ClaimDescriptorBuilder builder)
            : this(builder.Type, builder.Value, builder.ValueType, builder.Issuer, builder.OriginalIssuer)
        { }

        public override bool Equals(object obj) => Equals(obj as ClaimDescriptor);

        public bool Equals(ClaimDescriptor other)
        {
            return Type == other.Type
                && Value == other.Value
                && ValueType == other.ValueType
                && Issuer == other.Issuer
                && OriginalIssuer == other.OriginalIssuer;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Type.GetHashCode();
                if (null != Value)
                {
                    hash = hash * 23 + Value.GetHashCode();
                }
                if (null != ValueType)
                {
                    hash = hash * 23 + ValueType.GetHashCode();
                }
                if (null != Issuer)
                {
                    hash = hash * 23 + Issuer.GetHashCode();
                }
                if (null != OriginalIssuer)
                {
                    hash = hash * 23 + OriginalIssuer.GetHashCode();
                }
                return hash;
            }
        }

        public ClaimDescriptor Update(Action<ClaimDescriptorBuilder> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            var builder = new ClaimDescriptorBuilder(this);
            update(builder);
            return builder.Build();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Type), Type);
            info.AddValue(nameof(Value), Value);
            info.AddValue(nameof(ValueType), ValueType);
            info.AddValue(nameof(Issuer), Issuer);
            info.AddValue(nameof(OriginalIssuer), OriginalIssuer);
        }
    }
}