namespace NCoreUtils.Authentication
{
    public class PasswordLoginOptions
    {
        public static PasswordLoginOptions Default { get; } = new PasswordLoginOptionsBuilder().Build();

        public bool IsSensitiveLoggingEnabled { get; set; }

        public PasswordLoginOptions(PasswordLoginOptionsBuilder builder)
            => IsSensitiveLoggingEnabled = builder.IsSensitiveLoggingEnabled;
    }
}