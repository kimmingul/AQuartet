using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AQuartet.Core;
using Wpf.Ui.Appearance;

namespace AQuartet.App.Converters;

/// <summary>
/// Converts a service and theme into a themed logo image source.
/// </summary>
public sealed class ServiceThemeToLogoConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts the bound service and theme into a themed logo image source.
    /// </summary>
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2)
        {
            return Binding.DoNothing;
        }

        if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
        {
            return Binding.DoNothing;
        }

        if (values[0] is not AiService service || values[1] is not ApplicationTheme theme)
        {
            return Binding.DoNothing;
        }

        var variant = theme == ApplicationTheme.Light ? "light" : "dark";
        var themedUri = BuildLogoUri(service.Id, variant);
        var themedSource = TryLoadImage(themedUri);
        if (themedSource is not null)
        {
            return themedSource;
        }

        var fallbackSource = TryLoadImage(BuildLogoUri(service.Id, null));
        return fallbackSource ?? Binding.DoNothing;
    }

    /// <summary>
    /// One-way conversion only.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(targetTypes);

        var results = new object[targetTypes.Length];
        Array.Fill(results, Binding.DoNothing);
        return results;
    }

    private static Uri BuildLogoUri(string id, string? variant)
    {
        var suffix = string.IsNullOrWhiteSpace(variant) ? string.Empty : $".{variant}";
        return new Uri($"pack://application:,,,/Assets/Logos/{id}{suffix}.png", UriKind.Absolute);
    }

    private static ImageSource? TryLoadImage(Uri uri)
    {
        try
        {
            var resource = Application.GetResourceStream(uri);
            if (resource?.Stream is null)
            {
                return null;
            }

            using var stream = resource.Stream;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"[Logo Load] {ex.Message}");
            return null;
        }
    }
}
