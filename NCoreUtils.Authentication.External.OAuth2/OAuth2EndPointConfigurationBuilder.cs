namespace NCoreUtils.Authentication.OAuth2
{
    public class OAuth2EndPointConfigurationBuilder
    {
        public string Uri { get; set; }

        public OAuth2RequestMethod Method { get; set; } = OAuth2RequestMethod.Query;

        public OAuth2EndPointConfiguration Build() => new OAuth2EndPointConfiguration(this);
    }
}