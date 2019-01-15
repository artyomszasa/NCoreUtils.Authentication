namespace NCoreUtils.OAuth2.WebService.ViewModel
{
    public class OAuth2LoginData
    {
        public string RedirectUri { get; set; }

        public string ClientApplicationName { get; set; }

        public string Scopes { get; set; }

        public string State { get; set; }

        public int ClientApplicationId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}