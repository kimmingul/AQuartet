using System;
using System.Collections.Generic;
using System.Linq;

namespace AQuartet.Core;

public static class AiServiceRegistry
{
    // Common OAuth hosts shared across services
    private static readonly string[] GoogleOAuthHosts =
    [
        "accounts.google.com",
        "accounts.youtube.com",
        "myaccount.google.com",
        "apis.google.com",
        "ssl.gstatic.com",
        "accounts.gstatic.com"
    ];

    private static readonly string[] GoogleOAuthWildcards =
    [
        ".google.com",
        ".gstatic.com",
        ".googleapis.com",
        ".googleusercontent.com"
    ];

    private static readonly string[] AppleOAuthHosts =
    [
        "appleid.apple.com",
        "idmsa.apple.com"
    ];

    private static readonly string[] MicrosoftOAuthHosts =
    [
        "login.microsoftonline.com",
        "login.live.com"
    ];

    private static readonly string[] MicrosoftOAuthWildcards =
    [
        ".microsoftonline.com",
        ".live.com"
    ];

    public static IReadOnlyList<AiService> All { get; } = CreateServices();

    public static AiService GetById(string id) =>
        All.First(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

    public static AiService Default => All[0];

    private static AiService[] CreateServices()
    {
        return
        [
            new AiService(
                id: "grok",
                name: "Grok",
                homeUrl: "https://grok.com",
                sidebarLabel: "G",
                exactHosts: Combine(
                    ["grok.com", "www.grok.com", "grok.x.ai",
                     "x.ai", "www.x.ai", "auth.x.ai", "login.x.ai", "api.x.ai", "accounts.x.ai",
                     "x.com", "www.x.com", "twitter.com", "www.twitter.com", "t.co",
                     "grokusercontent.com", "www.grokusercontent.com",
                     "auth.grokipedia.com", "auth.grokusercontent.com"],
                    GoogleOAuthHosts, AppleOAuthHosts),
                wildcardSuffixes: Combine(
                    [".grok.com", ".x.ai", ".x.com", ".twitter.com",
                     ".grokipedia.com", ".grokusercontent.com"],
                    GoogleOAuthWildcards)),

            new AiService(
                id: "claude",
                name: "Claude",
                homeUrl: "https://claude.ai",
                sidebarLabel: "C",
                exactHosts: Combine(
                    ["claude.ai", "www.claude.ai"],
                    GoogleOAuthHosts, AppleOAuthHosts),
                wildcardSuffixes: Combine(
                    [".claude.ai", ".anthropic.com"],
                    GoogleOAuthWildcards)),

            new AiService(
                id: "chatgpt",
                name: "ChatGPT",
                homeUrl: "https://chatgpt.com",
                sidebarLabel: "GP",
                exactHosts: Combine(
                    ["chatgpt.com", "www.chatgpt.com",
                     "auth0.openai.com", "auth.openai.com"],
                    GoogleOAuthHosts, AppleOAuthHosts, MicrosoftOAuthHosts),
                wildcardSuffixes: Combine(
                    [".openai.com", ".oaiusercontent.com", ".chatgpt.com"],
                    GoogleOAuthWildcards, MicrosoftOAuthWildcards)),

            new AiService(
                id: "gemini",
                name: "Gemini",
                homeUrl: "https://gemini.google.com",
                sidebarLabel: "Gm",
                exactHosts: Combine(
                    ["gemini.google.com"],
                    GoogleOAuthHosts),
                wildcardSuffixes: Combine(
                    [".google.com", ".gstatic.com", ".googleapis.com", ".googleusercontent.com"],
                    Array.Empty<string>())),

            new AiService(
                id: "perplexity",
                name: "Perplexity",
                homeUrl: "https://www.perplexity.ai",
                sidebarLabel: "P",
                exactHosts: Combine(
                    ["perplexity.ai", "www.perplexity.ai"],
                    GoogleOAuthHosts, AppleOAuthHosts),
                wildcardSuffixes: Combine(
                    [".perplexity.ai"],
                    GoogleOAuthWildcards)),

            new AiService(
                id: "copilot",
                name: "Copilot",
                homeUrl: "https://copilot.microsoft.com",
                sidebarLabel: "Co",
                exactHosts: Combine(
                    ["copilot.microsoft.com", "m365.cloud.microsoft"],
                    MicrosoftOAuthHosts),
                wildcardSuffixes: Combine(
                    [".microsoft.com", ".live.com", ".microsoftonline.com", ".bing.com", ".cloud.microsoft"],
                    Array.Empty<string>()))
        ];
    }

    private static string[] Combine(params string[][] arrays)
    {
        return arrays.SelectMany(a => a).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
