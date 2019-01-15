namespace NCoreUtils.OAuth2
{
    public static class OAuth2ErrorMessages
    {
        public static readonly string InvalidAuthorizationCode = "Invalid authorization code.";

        public static readonly string InvalidRefreshToken = "Invalid refresh token.";

        public static readonly string InvalidUser = "Invalid user.";

        public static readonly string InvalidHost = "Invalid host.";

        public static readonly string InvalidClientApplication = "Invalid client application.";

        public static readonly string InvalidUserCredentials = "Invalid user credentials.";

        public static string Missing(string parameterName) => $"Missing {parameterName} parameter.";

        public static string Invalid(string parameterName) => $"Invalid {parameterName} parameter.";

        public static string UnsupportedGrantType(string grantType)
            => $"Unsupported {OAuth2Parameters.GrantType} parameter: {grantType}.";

        public static string UnsufficientPermissionsToGrant(string scope)
            => $"Requesting user has no permission to grant \"{scope}\" scope.";
    }
}