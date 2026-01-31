using System;
using System.Collections.Generic;

namespace AQuartet.Core;

public sealed class AiService
{
    public string Id { get; }
    public string Name { get; }
    public string HomeUrl { get; }
    public string SidebarLabel { get; }
    public string LogoResource { get; }
    public IReadOnlyList<string> ExactHosts { get; }
    public IReadOnlyList<string> WildcardSuffixes { get; }

    public AiService(
        string id,
        string name,
        string homeUrl,
        string sidebarLabel,
        IReadOnlyList<string> exactHosts,
        IReadOnlyList<string> wildcardSuffixes)
    {
        Id = id;
        Name = name;
        HomeUrl = homeUrl;
        SidebarLabel = sidebarLabel;
        LogoResource = $"pack://application:,,,/Assets/Logos/{id}.png";
        ExactHosts = exactHosts;
        WildcardSuffixes = wildcardSuffixes;
    }

    public bool IsHostAllowed(string host)
    {
        foreach (var h in ExactHosts)
        {
            if (string.Equals(host, h, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        foreach (var suffix in WildcardSuffixes)
        {
            if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
