using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OvoUi.Common;
using OvoUi.Common.Classes;
using OvoUi.Common.Enums;
using OvoUi.Controls.Options;

namespace OvoUi.Controls;

public static class OverlayDrawer
{
    public static void ShowStandard<TView, TViewModel>(TViewModel vm, string? hostId = null,
        DrawerOptions? options = null) where TView : Control, new() =>
        ShowStandard(new TView(), vm, hostId, options);

    public static void ShowStandard(Control control, object? vm, string? hostId = null, DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return;
        var drawer = new StandardDrawerControl { Content = control, DataContext = vm };
        Configure(drawer, options);
        host.AddDrawer(drawer);
    }

    public static void ShowStandard(object? vm, string? hostId = null, DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return;
        var view = BuildView(host, vm);
        var drawer = new StandardDrawerControl { Content = view, DataContext = vm };
        Configure(drawer, options);
        host.AddDrawer(drawer);
    }

    public static Task<DialogResult> ShowStandardAsync<TView, TViewModel>(TViewModel vm, string? hostId = null,
        DrawerOptions? options = null) where TView : Control, new() =>
        ShowStandardAsync(new TView(), vm, hostId, options);

    public static Task<DialogResult> ShowStandardAsync(Control control, object? vm, string? hostId = null,
        DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return Task.FromResult(DialogResult.None);
        var drawer = new StandardDrawerControl
        {
            Content = control, DataContext = vm,
            [KeyboardNavigation.TabNavigationProperty] = KeyboardNavigationMode.Cycle
        };
        Configure(drawer, options);
        host.AddModalDrawer(drawer);
        return drawer.ShowAsync<DialogResult>();
    }

    public static Task<DialogResult> ShowStandardAsync(object? vm, string? hostId = null, DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return Task.FromResult(DialogResult.None);
        return ShowStandardAsync(BuildView(host, vm), vm, hostId, options);
    }

    public static void ShowCustom<TView, TViewModel>(TViewModel vm, string? hostId = null,
        DrawerOptions? options = null) where TView : Control, new() =>
        ShowCustom(new TView(), vm, hostId, options);

    public static void ShowCustom(Control control, object? vm, string? hostId = null, DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return;
        var drawer = new CustomDrawerControl { Content = control, DataContext = vm };
        Configure(drawer, options);
        host.AddDrawer(drawer);
    }

    public static void ShowCustom(object? vm, string? hostId = null, DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return;
        var drawer = new CustomDrawerControl { Content = BuildView(host, vm), DataContext = vm };
        Configure(drawer, options);
        host.AddDrawer(drawer);
    }

    public static Task<TResult?> ShowCustomAsync<TView, TViewModel, TResult>(TViewModel vm, string? hostId = null,
        DrawerOptions? options = null) where TView : Control, new() =>
        ShowCustomAsync<TResult>(new TView(), vm, hostId, options);

    public static Task<TResult?> ShowCustomAsync<TResult>(Control control, object? vm, string? hostId = null,
        DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return Task.FromResult(default(TResult));
        var drawer = new CustomDrawerControl
        {
            Content = control, DataContext = vm,
            [KeyboardNavigation.TabNavigationProperty] = KeyboardNavigationMode.Cycle
        };
        Configure(drawer, options);
        host.AddModalDrawer(drawer);
        return drawer.ShowAsync<TResult?>();
    }

    public static Task<TResult?> ShowCustomAsync<TResult>(object? vm, string? hostId = null,
        DrawerOptions? options = null)
    {
        if (GetHost(hostId, options) is not { } host) return Task.FromResult(default(TResult));
        return ShowCustomAsync<TResult>(BuildView(host, vm), vm, hostId, options);
    }

    private static OverlayDialogHost? GetHost(string? hostId, DrawerOptions? options) =>
        OverlayDialogManager.GetHost(hostId, options?.TopLevelHashCode);

    private static Control BuildView(OverlayDialogHost host, object? vm)
    {
        var view = host.GetDataTemplate(vm)?.Build(vm) ?? new ContentControl { Padding = new Thickness(24) };
        view.DataContext = vm;
        return view;
    }

    private static void Configure(DrawerControlBase drawer, DrawerOptions? options)
    {
        options ??= DrawerOptions.Default;
        drawer.Position = options.Position;
        drawer.IsCloseButtonVisible = options.IsCloseButtonVisible;
        drawer.CanLightDismiss = options.CanLightDismiss;
        drawer.CanResize = options.CanResize;
        ScrollViewer.SetHorizontalScrollBarVisibility(drawer, options.HorizontalScrollBarVisibility);
        ScrollViewer.SetVerticalScrollBarVisibility(drawer, options.VerticalScrollBarVisibility);
        if (options.Position is Position.Left or Position.Right)
        {
            if (options.MinWidth is { } min) drawer.MinWidth = min;
            if (options.MaxWidth is { } max) drawer.MaxWidth = max;
        }
        else
        {
            if (options.MinHeight is { } min) drawer.MinHeight = min;
            if (options.MaxHeight is { } max) drawer.MaxHeight = max;
        }

        if (drawer is StandardDrawerControl standard)
        {
            standard.Buttons = options.Buttons;
            standard.Title = options.Title;
            standard.TitleCommand = options.TitleCommand;
        }

        if (!string.IsNullOrWhiteSpace(options.StyleClass))
            drawer.Classes.AddRange(options.StyleClass.Split(Constants.SpaceSeparator,
                StringSplitOptions.RemoveEmptyEntries));
    }
}