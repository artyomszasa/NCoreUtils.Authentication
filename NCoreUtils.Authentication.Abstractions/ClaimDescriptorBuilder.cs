namespace NCoreUtils.Authentication
{
    public sealed class ClaimDescriptorBuilder
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public string ValueType { get; set; }

        public string Issuer { get; set; }

        public string OriginalIssuer { get; set; }

        ClaimDescriptorBuilder(string type, string value, string valueType, string issuer, string originalIssuer)
        {
            Type = type;
            Value = value;
            ValueType = valueType;
            Issuer = issuer;
            OriginalIssuer = originalIssuer;
        }

        public ClaimDescriptorBuilder() : this(null, null, null, null, null) { }

        public ClaimDescriptorBuilder(ClaimDescriptor claimDescriptor)
            : this(claimDescriptor.Type, claimDescriptor.Value, claimDescriptor.ValueType, claimDescriptor.Issuer, claimDescriptor.OriginalIssuer)
        { }

        public ClaimDescriptor Build() => new ClaimDescriptor(this);

    }
}