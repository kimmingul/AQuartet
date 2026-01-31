using System;
using System.IO;

namespace AQuartet.Core;

public static class AppPaths
{
    public static string GetWebViewUserDataFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "AQuartet", "WebView2");
    }
}
