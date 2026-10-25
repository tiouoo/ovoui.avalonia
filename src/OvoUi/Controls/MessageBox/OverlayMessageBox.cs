using System;
using System.Threading.Tasks;
using Avalonia.Input;
using OvoUi.Common;

namespace OvoUi.Controls;

public static class OverlayMessageBox
{
    public static Task<MessageBoxResult> ShowAsync(string message, string? title = null, string? hostId = null,
        MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButton button = MessageBoxButton.OK,
        int? topLevelHashCode = null, string? styleClass = null)
    {
        var host = OverlayDialogManager.GetHost(hostId, topLevelHashCode);
        if (host is null) return Task.FromResult(MessageBoxResult.None);
        var control = new MessageBoxControl
        {
            Content = message,
            Title = title,
            Buttons = button,
            MessageIcon = icon,
            [KeyboardNavigation.TabNavigationProperty] = KeyboardNavigationMode.Cycle
        };
        if (!string.IsNullOrWhiteSpace(styleClass))
            control.Classes.AddRange(styleClass.Split(Constants.SpaceSeparator, StringSplitOptions.RemoveEmptyEntries));
        host.AddModalDialog(control);
        return control.ShowAsync<MessageBoxResult>();
    }
}