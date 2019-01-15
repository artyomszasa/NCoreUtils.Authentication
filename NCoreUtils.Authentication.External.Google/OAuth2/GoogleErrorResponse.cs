using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace NCoreUtils.Authentication.OAuth2
{
    public sealed class GoogleErrorResponse
    {
        const string RedirectUriMismatch = "redirect_uri_mismatch";

        const string InvalidRequest = "invalid_request";

        const string InvalidClient = "invalid_client";

        const string InvalidGrant = "invalid_grant";

        const string InvalidScope = "invalid_scope";

        const string UnauthorizedClient = "unauthorized_client";

        const string UnsupportedGrantType = "unsupported_grant_type";

        static readonly ImmutableDictionary<string, string> _messages = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { RedirectUriMismatch, "Invalid redirect uri has been specified." },
            { InvalidRequest, "Either missing or unsupported parameter." },
            { InvalidClient, "Invalid client ID or secret." },
            { InvalidGrant, "The authorization code is invalid or expired. " },
            { InvalidScope, "Invalid scope value." },
            { UnauthorizedClient, "Client is not authorized to use the requested grant type." },
            { UnsupportedGrantType, "Requested grant type is not supported." },
        }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }

        public override string ToString()
            => _messages.TryGetValue(Error, out var message) ? message : Description;
    }
}