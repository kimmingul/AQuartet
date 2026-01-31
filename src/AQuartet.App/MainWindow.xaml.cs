using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AQuartet.App.ViewModels;
using Wpf.Ui.Controls;

namespace AQuartet.App;

public partial class MainWindow : FluentWindow
{
    private Point _dragStartPoint;
    private TabViewModel? _draggedTab;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void TabHeader_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _draggedTab = GetTabFromElement(e.OriginalSource as DependencyObject);
    }

    private void TabHeader_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedTab is null)
        {
            return;
        }

        var diff = _dragStartPoint - e.GetPosition(null);
        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        DragDrop.DoDragDrop(TabHeaderList, _draggedTab, DragDropEffects.Move);
        _draggedTab = null;
    }

    private void TabHeader_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(TabViewModel)) is not TabViewModel droppedTab)
        {
            return;
        }

        var targetTab = GetTabFromElement(e.OriginalSource as DependencyObject);
        if (targetTab is null || targetTab == droppedTab || DataContext is not MainViewModel vm)
        {
            return;
        }

        var fromIndex = vm.Tabs.IndexOf(droppedTab);
        var toIndex = vm.Tabs.IndexOf(targetTab);
        vm.MoveTab(fromIndex, toIndex);
    }

    private static TabViewModel? GetTabFromElement(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is FrameworkElement { DataContext: TabViewModel tab })
            {
                return tab;
            }

            element = System.Windows.Media.VisualTreeHelper.GetParent(element);
        }

        return null;
    }
}
