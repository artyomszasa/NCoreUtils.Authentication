using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NCoreUtils.Authentication;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.WebService.Controllers
{
    public class OAuth2Controller : Controller
    {
        static readonly ImmutableHashSet<string> _ajaxMediaTypes = ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, new []
        {
            "application/json",
            "text/json",
            "application/xml",
            "text/xml"
        });

        static readonly ImmutableHashSet<string> _webMediaTypes = ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, new []
        {
            "text/html",
            "text/xhtml",
            "application/html",
            "application/xhtml",
            "application/xhtml+xml"
        });

        static readonly Regex _bearerRegex = new Regex("^Bearer\\s+(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        static bool TryGetBearerToken(string input, out string token)
        {
            var m = _bearerRegex.Match(input);
            if (m.Success)
            {
                token = m.Groups[1].Value;
                return true;
            }
            token = default(string);
            return false;
        }

        protected ILogger Logger { get; }

        public OAuth2Controller(ILogger<OAuth2Controller> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        IActionResult SendOAuth2Error(OAuth2Error error, string errorDescription = null, string redirectUri = null, string state = null)
        {
            var headers = Request.GetTypedHeaders();
            var ajaxMediaItem = headers.Accept.FirstOrDefault(mediaType => mediaType.MediaType.HasValue && _ajaxMediaTypes.Contains(mediaType.MediaType.Value));
            var webMediaItem = headers.Accept.FirstOrDefault(mediaType => mediaType.MediaType.HasValue && _webMediaTypes.Contains(mediaType.MediaType.Value));

            bool preferAjax;
            if (null == redirectUri)
            {
                preferAjax = true;
            }
            else
            {
                preferAjax = null != ajaxMediaItem && (null == webMediaItem || headers.Accept.IndexOf(ajaxMediaItem) < headers.Accept.IndexOf(webMediaItem));
            }
            if (preferAjax)
            {
                return new OAuth2ErrorResult(error, errorDescription);
            }
            return this.ErrorView(error, errorDescription);
        }

        public async Task<IActionResult> AuthenticateAsync(
            [FromServices] OAuth2Core core,
            [FromServices] IDataRepository<ClientApplication, int> clientApplicationRepository,
            [FromServices] IEncryptionProvider encryptionProvider,
            [FromForm] ViewModel.OAuth2LoginData loginData)
        {
            var clientApplication = await clientApplicationRepository.LookupAsync(loginData.ClientApplicationId, HttpContext.RequestAborted);
            if (null == clientApplication)
            {
                return this.ErrorView(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.InvalidClientApplication);
            }
            if (string.IsNullOrWhiteSpace(loginData.RedirectUri))
            {
                return this.ErrorView(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.RedirectUri));
            }
            if (!Uri.TryCreate(loginData.RedirectUri, UriKind.Absolute, out var uri))
            {
                return this.ErrorView(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Invalid(OAuth2Parameters.RedirectUri));
            }
            if (!clientApplication.Domains.Any(domain => domain.DomainName == uri.Host))
            {
                return this.ErrorView(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Invalid(OAuth2Parameters.RedirectUri));
            }
            string appendix;
            try
            {
                var code = await core.CreateAuthorizationCodeByPasswordAsync(
                    clientApplicationId: loginData.ClientApplicationId,
                    username: loginData.Username,
                    password: loginData.Password,
                    scopes: loginData.Scopes,
                    encryptionProvider: encryptionProvider,
                    cancellationToken: HttpContext.RequestAborted);
                appendix = $"code={Uri.EscapeDataString(code)}";
                if (!string.IsNullOrEmpty(loginData.State))
                {
                    appendix += $"&state={Uri.EscapeDataString(loginData.State)}";
                }
            }
            catch (OAuth2Exception exn)
            {
                appendix = $"error={Uri.EscapeDataString(exn.Error.Stringify())}";
                if (!string.IsNullOrWhiteSpace(exn.ErrorDescription))
                {
                    appendix += $"&error_description={Uri.EscapeDataString(exn.ErrorDescription)}";
                }
            }
            catch (Exception exn)
            {
                appendix = $"error={Uri.EscapeDataString(OAuth2Error.ServerError.Stringify())}&error_description={Uri.EscapeDataString(exn.Message)}";
            }
            var builder = new UriBuilder(uri);
            if (string.IsNullOrEmpty(builder.Query))
            {
                builder.Query = "?" + appendix;
            }
            else if (builder.Query.EndsWith('?'))
            {
                builder.Query += appendix;
            }
            else
            {
                builder.Query += "&" + appendix;
            }
            return Redirect(builder.Uri.ToString());
        }

        [ActionName("Token")]
        [HttpGet]
        public Task<IActionResult> TokenGetAsync(
            [ClientApplication] ClientApplication clientApplication,
            [FromServices] OAuth2Core core,
            [FromServices] CurrentClientApplication currentClientApplication,
            [FromServices] IEncryptionProvider encryptionProvider,
            [FromQuery(Name = OAuth2Parameters.GrantType)] string grantType,
            [FromQuery(Name = OAuth2Parameters.Username)] string username = null,
            [FromQuery(Name = OAuth2Parameters.Password)] string password = null,
            [FromQuery(Name = OAuth2Parameters.State)] string state = null,
            [FromQuery(Name = OAuth2Parameters.RefreshToken)] string refreshToken = null,
            [FromQuery(Name = OAuth2Parameters.RedirectUri)] string redirectUri = null,
            [FromQuery(Name = OAuth2Parameters.Scope)] string scopes = null,
            [FromQuery(Name = OAuth2Parameters.Code)] string code = null)
        {
            return TokenAsync(
                clientApplication,
                core,
                currentClientApplication,
                encryptionProvider,
                grantType,
                username,
                password,
                state,
                refreshToken,
                redirectUri,
                scopes,
                code);
        }


        [ActionName("Token")]
        [HttpPost]
        public async Task<IActionResult> TokenAsync(
            [ClientApplication] ClientApplication clientApplication,
            [FromServices] OAuth2Core core,
            [FromServices] CurrentClientApplication currentClientApplication,
            [FromServices] IEncryptionProvider encryptionProvider,
            [FromForm(Name = OAuth2Parameters.GrantType)] string grantType,
            [FromForm(Name = OAuth2Parameters.Username)] string username = null,
            [FromForm(Name = OAuth2Parameters.Password)] string password = null,
            [FromForm(Name = OAuth2Parameters.State)] string state = null,
            [FromForm(Name = OAuth2Parameters.RefreshToken)] string refreshToken = null,
            [FromForm(Name = OAuth2Parameters.RedirectUri)] string redirectUri = null,
            [FromForm(Name = OAuth2Parameters.Scope)] string scopes = null,
            [FromForm(Name = OAuth2Parameters.Code)] string code = null)
        {
            if (null == clientApplication)
            {
                return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.InvalidHost);
            }
            if (string.IsNullOrWhiteSpace(grantType))
            {
                return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.GrantType));
            }
            currentClientApplication.Id = clientApplication.Id;
            if (OAuth2GrantTypes.Password == grantType)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.Username));
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.Password));
                }
                var responseObject = await core.AuthenticateByPasswordAsync(clientApplication.Id, username, password, scopes, encryptionProvider, HttpContext.RequestAborted);
                return Json(responseObject);
            }
            if (OAuth2GrantTypes.RefreshToken == grantType)
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.RefreshToken));
                }
                var responseObject = await core.RefreshTokenAsync(refreshToken, encryptionProvider, HttpContext.RequestAborted);
                return new OkObjectResult(responseObject);
            }
            if (OAuth2GrantTypes.Code == grantType)
            {
                if (null == redirectUri)
                {
                    return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.RedirectUri));
                }
                if (null == code)
                {
                    return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.Missing(OAuth2Parameters.Code));
                }
                var responseObject = await core.AuthenticateByCodeAsync(clientApplication.Id, redirectUri, code, encryptionProvider, HttpContext.RequestAborted);
                return new OkObjectResult(responseObject);
                // Login form is shown
                // return View("Login", new ViewModel.OAuth2LoginData
                // {
                //     RedirectUri = redirectUri,
                //     ClientApplicationId = clientApplication.Id,
                //     ClientApplicationName = clientApplication.Name,
                //     State = state,
                //     Scopes = scopes
                // });
            }
            return new OAuth2ErrorResult(OAuth2Error.InvalidRequest, OAuth2ErrorMessages.UnsupportedGrantType(grantType));
        }

        // FIXME: implement authentication handler
        [ActionName("OpenID")]
        public async Task<IActionResult> OpenIdAsync(
            [FromServices] IEncryptionProvider encryptionProvider,
            [FromServices] IDataRepository<User, int> userRepository)
        {
            var headers = Request.Headers;
            if (headers.TryGetValue("Authorization", out var values) && values.Count > 0 && TryGetBearerToken(values.First(), out var token))
            {
                try
                {
                    var accessToken = await encryptionProvider.DecryptTokenFromBase64(token, HttpContext.RequestAborted);
                    if (accessToken.ExpiresAt > DateTimeOffset.Now)
                    {
                        if (int.TryParse(accessToken.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
                        {
                            var user = await userRepository.LookupAsync(userId, HttpContext.RequestAborted);
                            if (null != user)
                            {
                                return new OkObjectResult(new ViewModel.OpenIDUserInfo
                                {
                                    Id = userId,
                                    GivenName = user.GivenName,
                                    FamilyName = user.FamilyName,
                                    Email = user.Email,
                                    Scopes = accessToken.Scopes.ToArray()
                                });
                            }
                            else
                            {
                                Logger.LogTrace("No user found for id = {0}.", userId);
                            }
                        }
                        else
                        {
                            Logger.LogTrace("Invalid user id = \"{0}\".", accessToken.Id);
                        }
                    }
                    else
                    {
                        Logger.LogTrace("Expired access token for user id = \"{0}\".", accessToken.Id);
                    }
                }
                catch (Exception exn)
                {
                    Logger.LogTrace(exn, "Failed to decrypt access token.");
                }
            }
            else
            {
                Logger.LogTrace("No bearer token.");
            }
            return StatusCode(401);
        }
    }
}