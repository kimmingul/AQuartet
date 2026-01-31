using System;

namespace AQuartet.Core;

public static class NavigationPolicy
{
    public static bool ShouldAllow(Uri? uri, AiService? service = null)
    {
        if (uri is null || !uri.IsAbsoluteUri)
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps;
    }
}
