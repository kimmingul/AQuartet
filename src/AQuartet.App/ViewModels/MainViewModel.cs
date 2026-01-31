using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AQuartet.Core;
using Wpf.Ui.Appearance;

namespace AQuartet.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private TabViewModel? _selectedTab;

    public MainViewModel()
    {
        Services = AiServiceRegistry.All;
        Tabs = new ObservableCollection<TabViewModel>();

        // Open default service
        SelectService(AiServiceRegistry.Default);
    }

    public IReadOnlyList<AiService> Services { get; }

    public ObservableCollection<TabViewModel> Tabs { get; }

    public TabViewModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            var previous = _selectedTab;
            if (SetProperty(ref _selectedTab, value))
            {
                if (previous is not null) previous.IsSelected = false;
                if (value is not null) value.IsSelected = true;
                HookSelectedTab(value);
                OnPropertyChanged(nameof(CurrentUrl));
            }
        }
    }

    [ObservableProperty]
    private bool isAlwaysOnTop;

    [ObservableProperty]
    private ApplicationTheme themeMode = App.CurrentTheme;

    public string CurrentUrl => SelectedTab?.Address ?? string.Empty;

    public string ThemeIcon => ThemeMode switch
    {
        ApplicationTheme.Light => "☀️",
        ApplicationTheme.HighContrast => "🔳",
        _ => "🌙"
    };

    public string ThemeLabel => ThemeMode switch
    {
        ApplicationTheme.Light => "Theme: Light",
        ApplicationTheme.HighContrast => "Theme: HC",
        _ => "Theme: Dark"
    };

    [RelayCommand]
    private void SelectService(AiService? service)
    {
        if (service is null)
        {
            return;
        }

        // Find existing tab for this service
        var existing = Tabs.FirstOrDefault(t => t.Service.Id == service.Id);
        if (existing is not null)
        {
            SelectedTab = existing;
            return;
        }

        // Create new tab (one per service)
        var tab = new TabViewModel(service.HomeUrl, service);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private void CloseTab(TabViewModel? tab)
    {
        if (tab is null)
        {
            return;
        }

        var index = Tabs.IndexOf(tab);
        if (index < 0)
        {
            return;
        }

        tab.Detach();
        Tabs.RemoveAt(index);

        if (Tabs.Count == 0)
        {
            SelectService(AiServiceRegistry.Default);
            return;
        }

        SelectedTab = Tabs.ElementAtOrDefault(index) ?? Tabs.LastOrDefault();
    }

    [RelayCommand]
    private void Reload()
    {
        SelectedTab?.Reload();
    }

    [RelayCommand]
    private void NextTab()
    {
        if (Tabs.Count <= 1 || SelectedTab is null)
        {
            return;
        }

        var index = Tabs.IndexOf(SelectedTab);
        SelectedTab = Tabs[(index + 1) % Tabs.Count];
    }

    [RelayCommand]
    private void PrevTab()
    {
        if (Tabs.Count <= 1 || SelectedTab is null)
        {
            return;
        }

        var index = Tabs.IndexOf(SelectedTab);
        SelectedTab = Tabs[(index - 1 + Tabs.Count) % Tabs.Count];
    }

    public void MoveTab(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= Tabs.Count || toIndex < 0 || toIndex >= Tabs.Count || fromIndex == toIndex)
        {
            return;
        }

        Tabs.Move(fromIndex, toIndex);
    }

    [RelayCommand]
    private void CycleTheme()
    {
        App.CycleTheme();
        ThemeMode = App.CurrentTheme;
        OnPropertyChanged(nameof(ThemeLabel));
        OnPropertyChanged(nameof(ThemeIcon));
    }

    private void HookSelectedTab(TabViewModel? tab)
    {
        if (_selectedTab is not null)
        {
            _selectedTab.PropertyChanged -= OnSelectedTabPropertyChanged;
        }

        if (tab is not null)
        {
            tab.PropertyChanged += OnSelectedTabPropertyChanged;
        }
    }

    private void OnSelectedTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabViewModel.Address))
        {
            OnPropertyChanged(nameof(CurrentUrl));
        }
    }
}
