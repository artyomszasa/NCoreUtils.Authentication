using System;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.OAuth2.WebService
{
    public static class OAuth2RedirectResult
    {
        public static IActionResult Error(string redirectUri, OAuth2Error error, string errorDescription = null, string state = null)
        {
            string appendix = $"error={Uri.EscapeDataString(error.Stringify())}";
            if (null != errorDescription)
            {
                appendix += $"&error_description={Uri.EscapeDataString(errorDescription)}";
            }
            if (null != state)
            {
                appendix += $"&state={Uri.EscapeDataString(state)}";
            }
            string targetUri;
            if (redirectUri.Contains("?"))
            {
                if (redirectUri.EndsWith("?"))
                {
                    targetUri = redirectUri + appendix;
                }
                else
                {
                    targetUri = $"{redirectUri}&{appendix}";
                }
            }
            else
            {
                targetUri = $"{redirectUri}?{appendix}";
            }
            return new RedirectResult(targetUri, false);
        }
    }
}