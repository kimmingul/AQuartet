using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using AQuartet.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace AQuartet.App.ViewModels;

public partial class TabViewModel : ObservableObject
{
    private WebView2? _webView;
    private bool _isWebViewAttached;

    public AiService Service { get; }

    public TabViewModel(string initialUrl, AiService service)
    {
        Service = service;
        Address = initialUrl;
        Title = service.Name;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string address;

    [ObservableProperty]
    private bool canGoBack;

    [ObservableProperty]
    private bool canGoForward;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private BitmapImage? favicon;

    private static readonly HttpClient s_httpClient = new();

    public void AttachWebView(WebView2 webView)
    {
        if (_isWebViewAttached)
        {
            return;
        }

        _webView = webView;
        _isWebViewAttached = true;

        _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

        _webView.CoreWebView2.NavigationStarting += OnNavigationStarting;
        _webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        _webView.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        _webView.CoreWebView2.HistoryChanged += OnHistoryChanged;
        _webView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        _webView.CoreWebView2.FaviconChanged += OnFaviconChanged;

        UpdateHistoryState();
        UpdateFaviconAsync();
    }

    public void Detach()
    {
        if (!_isWebViewAttached || _webView?.CoreWebView2 is null)
        {
            return;
        }

        _webView.CoreWebView2.NavigationStarting -= OnNavigationStarting;
        _webView.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
        _webView.CoreWebView2.DocumentTitleChanged -= OnDocumentTitleChanged;
        _webView.CoreWebView2.HistoryChanged -= OnHistoryChanged;
        _webView.CoreWebView2.NewWindowRequested -= OnNewWindowRequested;
        _webView.CoreWebView2.FaviconChanged -= OnFaviconChanged;

        _webView = null;
        _isWebViewAttached = false;
    }

    private void OnFaviconChanged(object? sender, object e)
    {
        UpdateFaviconAsync();
    }

    private async void UpdateFaviconAsync()
    {
        if (_webView?.CoreWebView2 is null)
        {
            return;
        }

        var faviconUri = _webView.CoreWebView2.FaviconUri;
        if (string.IsNullOrEmpty(faviconUri))
        {
            Favicon = null;
            return;
        }

        try
        {
            var bytes = await s_httpClient.GetByteArrayAsync(faviconUri);
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(bytes))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            Favicon = bitmap;
        }
        catch
        {
            Favicon = null;
        }
    }

    public void Navigate(string url)
    {
        if (_webView is null || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (!TryNormalizeUrl(url, out var normalized))
        {
            return;
        }

        try
        {
            _webView.CoreWebView2.Navigate(normalized);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Navigate Error] {ex.Message}");
        }
    }

    public void Reload()
    {
        _webView?.Reload();
    }

    public void GoBack()
    {
        if (_webView?.CanGoBack == true)
        {
            _webView.GoBack();
        }
    }

    public void GoForward()
    {
        if (_webView?.CanGoForward == true)
        {
            _webView.GoForward();
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        IsLoading = true;

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) || !NavigationPolicy.ShouldAllow(uri, Service))
        {
            e.Cancel = true;
            OpenExternal(uri);
            return;
        }

        Address = e.Uri;
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        IsLoading = false;
        Address = _webView?.Source?.ToString() ?? Address;
        UpdateHistoryState();

        if (!e.IsSuccess && e.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
        {
            Debug.WriteLine($"[Navigation Error] {e.WebErrorStatus} for {Address}");
        }
    }

    private void OnDocumentTitleChanged(object? sender, object e)
    {
        Title = _webView?.CoreWebView2.DocumentTitle ?? Service.Name;
    }

    private void OnHistoryChanged(object? sender, object e)
    {
        UpdateHistoryState();
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (_webView?.CoreWebView2 is null)
        {
            return;
        }

        e.Handled = true;

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
        {
            return;
        }

        if (NavigationPolicy.ShouldAllow(uri, Service))
        {
            try
            {
                _webView.CoreWebView2.Navigate(uri.ToString());
                Address = uri.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Navigate Error] {ex.Message}");
            }
            return;
        }

        OpenExternal(uri);
    }

    private void UpdateHistoryState()
    {
        if (_webView is null)
        {
            CanGoBack = false;
            CanGoForward = false;
            return;
        }

        CanGoBack = _webView.CanGoBack;
        CanGoForward = _webView.CanGoForward;
    }

    private static void OpenExternal(Uri? uri)
    {
        if (uri is null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }
        catch
        {
            // Ignore external open failures.
        }
    }

    private static bool TryNormalizeUrl(string input, out string normalized)
    {
        normalized = input.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absolute))
        {
            normalized = absolute.ToString();
            return true;
        }

        if (Uri.TryCreate($"https://{normalized}", UriKind.Absolute, out var withScheme))
        {
            normalized = withScheme.ToString();
            return true;
        }

        return false;
    }
}
