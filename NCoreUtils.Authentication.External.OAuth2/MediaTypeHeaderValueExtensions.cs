using System;
using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace NCoreUtils.Authentication.OAuth2
{
    public static class MediaTypeHeaderValueExtensions
    {
        static readonly ImmutableHashSet<string> _jsonMediaTypes = ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, new []
        {
            "application/json",
            "text/json"
        });

        public static bool IsJson(this MediaTypeHeaderValue value)
        {
            if (null == value || null == value.MediaType)
            {
                return false;
            }
            return _jsonMediaTypes.Contains(value.MediaType);
        }
    }
}