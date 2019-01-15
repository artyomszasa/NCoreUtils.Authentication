using System.Collections.Generic;
using System.Collections.Immutable;

namespace NCoreUtils.OAuth2
{
    public static class OAuth2ErrorExtensions
    {
        static readonly ImmutableDictionary<OAuth2Error, string> _errorNames = new Dictionary<OAuth2Error, string>
        {
            { OAuth2Error.InvalidRequest, "invalid_request" },
            { OAuth2Error.UnauthorizedClient, "unauthorized_client"},
            { OAuth2Error.AccessDenied, "access_denied"},
            { OAuth2Error.UnsupportedResponseType, "unsupported_response_type" },
            { OAuth2Error.InvalidScope, "invalid_scope"},
            { OAuth2Error.InvalidGrant, "invalid_grant"},
            { OAuth2Error.ServerError, "server_error" },
            { OAuth2Error.TemporarilyUnavailable, "temporarily_unavailable" }
        }.ToImmutableDictionary();

        public static string Stringify(this OAuth2Error error)
            => _errorNames.GetValueOrDefault(error, "unknown_error");
    }
}