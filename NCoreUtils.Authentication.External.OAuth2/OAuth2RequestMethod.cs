namespace NCoreUtils.Authentication.OAuth2
{
    public enum OAuth2RequestMethod
    {
        /// <summary>
        /// Parameters should be passed in query string of GET request.
        /// </summary>
        Query,
        /// <summary>
        /// Parameters should be passed in form encoded body of POST request.
        /// </summary>
        Form
    }
}