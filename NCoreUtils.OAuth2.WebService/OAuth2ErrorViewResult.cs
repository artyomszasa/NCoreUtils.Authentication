using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.OAuth2.WebService
{
    public static class OAuth2ErrorViewResult
    {
        // public OAuth2ErrorViewResult(OAuth2Error error, string errorDescription = null)
        //     : base()
        // {
        //     ViewName = "Error";
        //     Model = new ViewModel.OAuth2Error { Error = error.Stringify(), ErrorDescription = errorDescription };
        // }

        public static IActionResult ErrorView(this Controller controller, OAuth2Error error, string errorDescription = null)
        {
            return controller.View("Error", new ViewModel.OAuth2Error { Error = error.Stringify(), ErrorDescription = errorDescription });
        }
    }
}