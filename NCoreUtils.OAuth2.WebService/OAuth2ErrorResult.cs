using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.OAuth2.WebService
{
    class OAuth2ErrorResult : OkObjectResult
    {
        public OAuth2ErrorResult(OAuth2Error error, string errorDescription = null)
            : base(new ViewModel.OAuth2Error { Error = error.Stringify(), ErrorDescription = errorDescription })
        { }
    }
}