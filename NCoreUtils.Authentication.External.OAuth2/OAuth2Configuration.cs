namespace NCoreUtils.Authentication.OAuth2
{
    public class OAuth2Configuration : IOAuth2Configuration
    {
        public OAuth2EndPointConfiguration AccessTokenEndPoint { get; }

        public OAuth2EndPointConfiguration UserInfoEndPoint { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string RedirectUri { get; }

        public OAuth2Configuration(OAuth2ConfigurationBuilder builder)
        {
            AccessTokenEndPoint = builder.AccessTokenEndPoint.Build();
            UserInfoEndPoint = builder.UserInfoEndPoint.Build();
            ClientId = builder.ClientId;
            ClientSecret = builder.ClientSecret;
            RedirectUri = builder.RedirectUri;
        }
    }

    public class OAuth2Configuration<TLogin> : OAuth2Configuration
    {
        public OAuth2Configuration(OAuth2ConfigurationBuilder builder) : base(builder) { }
    }
}