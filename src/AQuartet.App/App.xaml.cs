using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Appearance;

namespace AQuartet.App;

public partial class App : Application
{
    private static ApplicationTheme _currentTheme = ApplicationTheme.Dark;

    public static ApplicationTheme CurrentTheme => _currentTheme;

    public static void SetTheme(ApplicationTheme theme)
    {
        _currentTheme = theme;
        ApplicationThemeManager.Apply(theme);
    }

    public static void CycleTheme()
    {
        _currentTheme = _currentTheme switch
        {
            ApplicationTheme.Dark => ApplicationTheme.Light,
            ApplicationTheme.Light => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };
        ApplicationThemeManager.Apply(_currentTheme);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[Unhandled UI Exception] {args.Exception}");
            MessageBox.Show(
                $"예기치 않은 오류가 발생했습니다.\n\n{args.Exception.Message}",
                "A-Quartet",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                Debug.WriteLine($"[Unhandled Domain Exception] {ex}");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Debug.WriteLine($"[Unobserved Task Exception] {args.Exception}");
            args.SetObserved();
        };

        ApplicationThemeManager.Apply(_currentTheme);
    }
}
