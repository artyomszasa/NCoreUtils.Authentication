namespace NCoreUtils.Authentication.OAuth2
{
    public interface IOAuth2Configuration
    {
        OAuth2EndPointConfiguration AccessTokenEndPoint { get; }

        OAuth2EndPointConfiguration UserInfoEndPoint { get; }

        string ClientId { get; }

        string ClientSecret { get; }

        string RedirectUri { get; }
    }
}