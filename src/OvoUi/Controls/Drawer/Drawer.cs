using Avalonia.Controls;
using OvoUi.Common.Classes;
using OvoUi.Common.Enums;
using OvoUi.Controls.Options;

namespace OvoUi.Controls;

[Obsolete("Use OverlayDrawer instead.")]
public static class Drawer
{
    public static void ShowStandard<TView, TViewModel>(TViewModel vm, string? hostId = null, DrawerOptions? options = null) where TView : Control, new() => OverlayDrawer.ShowStandard<TView, TViewModel>(vm, hostId, options);
    public static Task<DialogResult> ShowStandardModal<TView, TViewModel>(TViewModel vm, string? hostId = null, DrawerOptions? options = null) where TView : Control, new() => OverlayDrawer.ShowStandardAsync<TView, TViewModel>(vm, hostId, options);
    public static void ShowCustom<TView, TViewModel>(TViewModel vm, string? hostId = null, DrawerOptions? options = null) where TView : Control, new() => OverlayDrawer.ShowCustom<TView, TViewModel>(vm, hostId, options);
    public static Task<TResult?> ShowCustomModal<TView, TViewModel, TResult>(TViewModel vm, string? hostId = null, DrawerOptions? options = null) where TView : Control, new() => OverlayDrawer.ShowCustomAsync<TView, TViewModel, TResult>(vm, hostId, options);
}
