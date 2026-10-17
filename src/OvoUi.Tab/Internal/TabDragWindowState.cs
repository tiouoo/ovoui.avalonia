using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace OvoUi.Tab.Internal;

/// <summary>Temporarily conceals a floating window without unmapping its capture handle.</summary>
internal sealed class TabDragWindowState(Window window)
{
    private readonly bool _showActivated = window.ShowActivated;
    private readonly bool _topmost = window.Topmost;
    private nint _cloakedHandle;
    private bool _hidden;
    private double _opacity;
    private WindowDecorations _decorations;
    private bool _canResize;
    private bool _extendClientArea;
    private IReadOnlyList<WindowTransparencyLevel>? _transparencyLevels;

    internal void HideWhileAttached()
    {
        if (_hidden) return;
        _hidden = true;

        // Visual.Opacity only affects Avalonia content. DWM cloaking conceals the native
        // frame, shadow and backdrop as well, without Hide()/Close() releasing capture.
        if (OperatingSystem.IsWindows() && window.TryGetPlatformHandle() is
                { HandleDescriptor: "HWND", Handle: var handle } && handle != 0 && SetCloak(handle, true))
        {
            _cloakedHandle = handle;
            window.SetCurrentValue(Window.TopmostProperty, _topmost);
            return;
        }

        _opacity = window.Opacity;
        _decorations = window.WindowDecorations;
        _canResize = window.CanResize;
        _extendClientArea = window.ExtendClientAreaToDecorationsHint;
        _transparencyLevels = window.TransparencyLevelHint;
        window.SetCurrentValue(Window.WindowDecorationsProperty, WindowDecorations.None);
        window.SetCurrentValue(Window.CanResizeProperty, false);
        window.SetCurrentValue(Window.ExtendClientAreaToDecorationsHintProperty, false);
        window.SetCurrentValue(TopLevel.TransparencyLevelHintProperty,
            new[] { WindowTransparencyLevel.Transparent });
        window.SetCurrentValue(Visual.OpacityProperty, 0d);
        window.SetCurrentValue(Window.TopmostProperty, _topmost);
    }

    internal void RestoreVisibility()
    {
        if (!_hidden || window.PlatformImpl is null) return;
        _hidden = false;
        if (_cloakedHandle != 0)
        {
            if (window.TryGetPlatformHandle()?.Handle == _cloakedHandle) SetCloak(_cloakedHandle, false);
            _cloakedHandle = 0;
            return;
        }

        window.SetCurrentValue(Window.WindowDecorationsProperty, _decorations);
        window.SetCurrentValue(Window.CanResizeProperty, _canResize);
        window.SetCurrentValue(Window.ExtendClientAreaToDecorationsHintProperty, _extendClientArea);
        window.SetCurrentValue(TopLevel.TransparencyLevelHintProperty, _transparencyLevels!);
        window.SetCurrentValue(Visual.OpacityProperty, _opacity);
    }

    internal void Restore()
    {
        if (window.PlatformImpl is null) return;
        RestoreVisibility();
        window.SetCurrentValue(Window.ShowActivatedProperty, _showActivated);
        window.SetCurrentValue(Window.TopmostProperty, _topmost);
    }

    private static bool SetCloak(nint handle, bool cloaked)
    {
        const int dwmwaCloak = 13;
        var value = cloaked ? 1 : 0;
        return DwmSetWindowAttribute(handle, dwmwaCloak, ref value, sizeof(int)) == 0;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
}
