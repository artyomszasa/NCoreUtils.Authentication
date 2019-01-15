namespace NCoreUtils.Authentication.OAuth2
{
    public interface IOAuth2AccessTokenResponse
    {
        string AccessToken { get; }

        string TokenType { get; }

        long ExpiresIn { get; }
    }
}