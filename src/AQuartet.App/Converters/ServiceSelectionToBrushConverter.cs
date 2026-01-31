using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AQuartet.Core;
using AQuartet.App.ViewModels;

namespace AQuartet.App.Converters;

public sealed class ServiceSelectionToBrushConverter : IMultiValueConverter
{
    public Brush? DefaultBrush { get; set; }

    public Brush? SelectedBrush { get; set; }

    /// <summary>
    /// Uses the selected tab to determine the appropriate brush for service icons.
    /// </summary>
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);

        var defaultBrush = DefaultBrush ?? Brushes.Gray;

        if (values.Length < 2)
        {
            return defaultBrush;
        }

        var service = values[0] as AiService;
        var selectedTab = values[1] as TabViewModel;

        if (service is null)
        {
            return defaultBrush;
        }

        if (selectedTab?.Service is not null && string.Equals(selectedTab.Service.Id, service.Id, StringComparison.OrdinalIgnoreCase))
        {
            return SelectedBrush ?? defaultBrush;
        }

        return defaultBrush;
    }

    /// <summary>
    /// One-way conversion only.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Service selection brush conversion is one-way.");
    }
}
