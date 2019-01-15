namespace NCoreUtils.Authentication
{
    public class PasswordLoginOptionsBuilder
    {
        public bool IsSensitiveLoggingEnabled { get; set; }

        internal PasswordLoginOptionsBuilder() { }

        public PasswordLoginOptionsBuilder EnableSensitiveLogging(bool isEnabled)
        {
            IsSensitiveLoggingEnabled = isEnabled;
            return this;
        }

        public PasswordLoginOptions Build() => new PasswordLoginOptions(this);
    }
}