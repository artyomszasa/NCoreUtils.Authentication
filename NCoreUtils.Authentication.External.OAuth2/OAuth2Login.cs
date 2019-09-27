using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// using NCoreUtils.Authentication.Internal;
using NCoreUtils.Authentication.OAuth2;
using Newtonsoft.Json;

namespace NCoreUtils.Authentication.OAuth2
{
    public abstract class OAuth2Login<TConfiguration, TAccessTokenResponse, TUserInfoResponse> : ILogin
        where TConfiguration : class, IOAuth2Configuration
        where TAccessTokenResponse : class, IOAuth2AccessTokenResponse
        where TUserInfoResponse : class, IOAuth2UserInfoResponse
    {
        protected static string AccessTokenPrefix { get; } = "accessToken";

        protected ILogger Logger { get; }

        protected TConfiguration Configuration { get; }

        protected IUsernameFormatter UsernameFormatter { get; }

        public OAuth2Login(
            ILogger<OAuth2Login<TConfiguration, TAccessTokenResponse, TUserInfoResponse>> logger,
            TConfiguration configuration,
            IUsernameFormatter usernameFormatter = null)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            UsernameFormatter = usernameFormatter ?? DefaultUsernameFormatter.SharedInstance;
        }

        protected virtual TAccessTokenResponse ParseAccessTokenResponse(string response)
            => JsonConvert.DeserializeObject<TAccessTokenResponse>(response);

        protected virtual TUserInfoResponse ParseUserInfoResponse(string response)
            => JsonConvert.DeserializeObject<TUserInfoResponse>(response);

        #region Communication
        // TODO: generic request message creation
        protected HttpRequestMessage CreateQueryRequestMessage(string endpoint, IEnumerable<KeyValuePair<string, string>> values)
        {
            var builder = new UriBuilder(endpoint);
            var firstArgument = true;
            var queryBuilder = new StringBuilder();
            foreach (var kv in values)
            {
                var key = kv.Key;
                var value = kv.Value;
                if (firstArgument)
                {
                    queryBuilder.Append('?');
                    firstArgument = false;
                }
                else
                {
                    queryBuilder.Append('&');
                }
                queryBuilder.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
            }
            builder.Query = queryBuilder.ToString();
            return new HttpRequestMessage(HttpMethod.Get, builder.Uri);
        }

        protected HttpRequestMessage CreateFormRequestMessage(string endpoint, IEnumerable<KeyValuePair<string, string>> values)
        {
            return new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(values)
            };
        }

        protected HttpRequestMessage CreateRequestMessage(OAuth2EndPointConfiguration endpoint, IEnumerable<KeyValuePair<string, string>> values)
        {
            switch (endpoint.Method)
            {
                case OAuth2RequestMethod.Query:
                    return CreateQueryRequestMessage(endpoint.Uri, values);
                case OAuth2RequestMethod.Form:
                    return CreateFormRequestMessage(endpoint.Uri, values);
                default:
                    throw new InvalidOperationException($"Invalid oauth2 request method.");
            }
        }

        #endregion

        protected virtual IEnumerable<KeyValuePair<string, string>> GetAccessTokenRequestParameters(string code)
        {
            yield return new KeyValuePair<string, string>("code", code);
            yield return new KeyValuePair<string, string>("client_id", Configuration.ClientId);
            yield return new KeyValuePair<string, string>("client_secret", Configuration.ClientSecret);
            yield return new KeyValuePair<string, string>("redirect_uri", Configuration.RedirectUri);
            yield return new KeyValuePair<string, string>("grant_type", "authorization_code");
        }

        protected virtual IEnumerable<KeyValuePair<string, string>> GetUserInfoRequestParameters(string accessToken)
        {
            yield return new KeyValuePair<string, string>("access_token", accessToken);
        }

        protected virtual HttpRequestMessage CreateAccessTokenRequestMessage(string code)
            => CreateRequestMessage(Configuration.AccessTokenEndPoint, GetAccessTokenRequestParameters(code));

        protected virtual HttpRequestMessage CreateUserInfoRequestMessage(string accessToken)
            => CreateRequestMessage(Configuration.UserInfoEndPoint, GetUserInfoRequestParameters(accessToken));


        protected virtual HttpClient CreateHttpClient() => new HttpClient();

        protected virtual async ValueTask<string> FormatInvalidResponseMessage(HttpResponseMessage response)
        {
            if (null != response.Content && response.Content.Headers.ContentType.IsJson())
            {
                var json = await response.Content.ReadAsStringAsync();
                return $"Server responded with status code {response.StatusCode}, json playload = {json}.";
            }
            return $"Server responded with status code {response.StatusCode}.";
        }

        protected virtual ValueTask<string> FormatInvalidAccessTokenResponseMessage(HttpResponseMessage response) => FormatInvalidResponseMessage(response);

        protected virtual async ValueTask<TAccessTokenResponse> GetAccessTokenAsync(string code, CancellationToken cancellationToken)
        {
            using (var request = CreateAccessTokenRequestMessage(code))
            using (var client = CreateHttpClient())
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType;
                    if (contentType.IsJson())
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var oauth2response = ParseAccessTokenResponse(json);
                        return oauth2response;
                    }
                    throw new OAuth2CodeValidationException(request.RequestUri, code, $"Server responded with unsupported content type {contentType}.");
                }
                throw new OAuth2CodeValidationException(request.RequestUri, code, await FormatInvalidAccessTokenResponseMessage(response));
            }
        }

        protected virtual ValueTask<string> FormatInvalidUserInfoResponseMessage(HttpResponseMessage response) => FormatInvalidResponseMessage(response);

        protected virtual async ValueTask<TUserInfoResponse> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
        {
            using (var request = CreateUserInfoRequestMessage(accessToken))
            using (var client = CreateHttpClient())
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType;
                    if (contentType.IsJson())
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var infoResponse = ParseUserInfoResponse(json);
                        return infoResponse;
                    }
                    throw new OAuth2UserInfoException(request.RequestUri, accessToken, $"Server responded with unsupported content type {contentType}.");
                }
                throw new OAuth2UserInfoException(request.RequestUri, accessToken, await FormatInvalidUserInfoResponseMessage(response));
            }
        }

        protected virtual IAsyncEnumerable<ClaimDescriptor> GetClaimsAsync(TUserInfoResponse user)
        {
            var name = UsernameFormatter.Format(user);
            return new List<ClaimDescriptor>
            {
                new ClaimDescriptor(ClaimTypes.Sid, user.ExternalId),
                new ClaimDescriptor(ClaimTypes.Email, user.Email),
                new ClaimDescriptor(ClaimTypes.GivenName, user.GivenName),
                new ClaimDescriptor(ClaimTypes.Surname, user.FamilyName),
                new ClaimDescriptor(ClaimTypes.Name, string.IsNullOrWhiteSpace(name) ? user.Email : name)
            }.ToAsyncEnumerable();
        }

        public async ValueTask<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken))
        {
            string accessToken;
            if (passcode.StartsWith($"{AccessTokenPrefix}!"))
            {
                accessToken = passcode.Substring(AccessTokenPrefix.Length + 1);
            }
            else
            {
                Logger.LogTrace("Attempting oauth2 login using code = {0}.", passcode);
                if (string.IsNullOrWhiteSpace(passcode))
                {
                    throw new ArgumentException("Must be a valid code string.", nameof(passcode));
                }
                Logger.LogTrace("Attempting to request oauth2 access token using code = {0}.", passcode);
                var accessTokenResponse = await GetAccessTokenAsync(passcode, cancellationToken).ConfigureAwait(false);
                if (null == accessTokenResponse)
                {
                    Logger.LogDebug("Failed to get oauth2 access token from code = {0}.", passcode);
                    return null;
                }
                accessToken = accessTokenResponse.AccessToken;
            }
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Logger.LogDebug("Empty oauth2 access token for passcode = {0}", passcode);
                return null;
            }
            Logger.LogTrace("Attempting get user info using access token = {0}.", accessToken);
            var userInfoResponse = await GetUserInfoAsync(accessToken, cancellationToken).ConfigureAwait(false);
            if (null == userInfoResponse)
            {
                Logger.LogDebug("Unable to retrieve user info for passcode = {0}", passcode);
                return null;
            }
            var claims = await GetClaimsAsync(userInfoResponse).ToListAsync(cancellationToken).ConfigureAwait(false);
            return new ClaimCollection(claims, "login", ClaimTypes.Name, ClaimTypes.Role);
        }
    }
}