using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AQuartet.App.ViewModels;
using AQuartet.Core;
using Microsoft.Web.WebView2.Core;

namespace AQuartet.App.Views;

public partial class TabView : UserControl
{
    private static readonly Task<CoreWebView2Environment> EnvironmentTask =
        CoreWebView2Environment.CreateAsync(null, AppPaths.GetWebViewUserDataFolder());

    private bool _coreReady;
    private Task? _initTask;

    public TabView()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_coreReady)
        {
            return;
        }

        await EnsureCoreReadyAsync();

        if (_coreReady && DataContext is TabViewModel vm)
        {
            vm.AttachWebView(WebView);

            if (!string.IsNullOrWhiteSpace(vm.Address) && WebView.CoreWebView2 is not null)
            {
                WebView.CoreWebView2.Navigate(vm.Address);
            }
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TabViewModel vm)
        {
            vm.Detach();
        }

        WebView?.Dispose();
        _coreReady = false;
    }

    private async Task EnsureCoreReadyAsync()
    {
        if (_coreReady)
        {
            return;
        }

        if (_initTask is not null)
        {
            await _initTask;
            return;
        }

        _initTask = InitializeCoreAsync();
        await _initTask;
    }

    private async Task InitializeCoreAsync()
    {
        try
        {
            var environment = await EnvironmentTask;
            if (WebView.CoreWebView2 is null)
            {
                await WebView.EnsureCoreWebView2Async(environment);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WebView2 Init Error] {ex}");
            MessageBox.Show(
                $"WebView2를 초기화할 수 없습니다.\n\n{ex.Message}\n\nWebView2 런타임이 설치되어 있는지 확인해 주세요.",
                "AI Guild Desktop",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _coreReady = true;
    }
}
