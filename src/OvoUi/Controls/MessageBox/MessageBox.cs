using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OvoUi.Common;

namespace OvoUi.Controls;

public static class MessageBox
{
    public static Task<MessageBoxResult> ShowAsync(string message, string? title = null,
        MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButton button = MessageBoxButton.OK,
        string? styleClass = null)
    {
        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        return owner is null
            ? ShowWithoutOwner(message, title, icon, button, styleClass)
            : ShowAsync(owner, message, title ?? string.Empty, icon, button, styleClass);
    }

    public static Task<MessageBoxResult> ShowAsync(Window owner, string message, string title,
        MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButton button = MessageBoxButton.OK,
        string? styleClass = null)
    {
        var window = Create(message, title, icon, button, styleClass);
        window.Icon = owner.Icon;
        ApplyOwnerAppearance(window, owner);
        return window.ShowDialog<MessageBoxResult>(owner);
    }

    private static Task<MessageBoxResult> ShowWithoutOwner(string message, string? title, MessageBoxIcon icon,
        MessageBoxButton button, string? styleClass)
    {
        Create(message, title, icon, button, styleClass).Show();
        return Task.FromResult(MessageBoxResult.None);
    }

    private static MessageBoxWindow Create(string message, string? title, MessageBoxIcon icon, MessageBoxButton button,
        string? styleClass)
    {
        var window = new MessageBoxWindow(button) { Content = message, Title = title, MessageIcon = icon };
        if (!string.IsNullOrWhiteSpace(styleClass))
            window.Classes.AddRange(styleClass.Split(Constants.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries));
        return window;
    }

    private static void ApplyOwnerAppearance(MessageBoxWindow window, Window owner)
    {
        window.FontFamily = owner.FontFamily;
        window.FontSize = owner.FontSize;
        window.FontWeight = owner.FontWeight;
        window.FlowDirection = owner.FlowDirection;
        window.RequestedThemeVariant = owner.ActualThemeVariant;
    }
}