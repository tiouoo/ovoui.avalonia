using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;

namespace OvoUi.Platform.MacOS;

/// <summary>
/// Identifies one of the three native macOS window controls.
/// </summary>
public enum MacOsWindowButton
{
    Close,
    Minimize,
    Zoom
}

/// <summary>
/// Configures one native macOS window control.
/// </summary>
public sealed class MacOsWindowButtonOptions
{
    /// <summary>
    /// Overrides the position calculated from the common origin and spacing.
    /// </summary>
    public Point? Position { get; set; }

    /// <summary>
    /// Overrides the native button size. Leave null to retain the system size.
    /// </summary>
    public Size? Size { get; set; }

    public double Opacity { get; set; } = 1;
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Configures the native close, minimize, and zoom controls of a macOS window.
/// </summary>
public sealed class MacOsWindowControlsOptions
{
    public double OriginX { get; set; } = 14;
    public double OriginY { get; set; } = 2;
    public double Spacing { get; set; } = 20;

    public MacOsWindowButtonOptions Close { get; set; } = new();
    public MacOsWindowButtonOptions Minimize { get; set; } = new();
    public MacOsWindowButtonOptions Zoom { get; set; } = new();

    internal (MacOsWindowButtonOptions Options, Point Position) GetButton(MacOsWindowButton button)
    {
        var index = (int)button;
        var options = button switch
        {
            MacOsWindowButton.Close => Close,
            MacOsWindowButton.Minimize => Minimize,
            MacOsWindowButton.Zoom => Zoom,
            _ => throw new ArgumentOutOfRangeException(nameof(button))
        };
        return (options, options.Position ?? new Point(OriginX + Spacing * index, OriginY));
    }
}

/// <summary>
/// Adjusts the native standard buttons of macOS windows without external dependencies.
/// </summary>
public static class MacOsWindowControls
{
    private static readonly ConditionalWeakTable<Window, NativeWindowState> InitialStates = new();

    public static bool IsSupported => OperatingSystem.IsMacOS();

    /// <summary>
    /// Applies the options when the window has an available macOS native handle.
    /// This method is safe to call on every platform.
    /// </summary>
    public static bool TryApply(Window window, MacOsWindowControlsOptions options, out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(options);

        if (!IsSupported)
        {
            error = "Native macOS window controls are only available on macOS.";
            return false;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.Handle == IntPtr.Zero)
        {
            error = "The window does not have a native handle yet. Apply the options after the window is opened.";
            return false;
        }

        try
        {
            InitialStates.GetValue(window, _ => CaptureState(handle.Handle));
            Apply(handle.Handle, options);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Restores the standard buttons to the state captured before the first call to <see cref="TryApply"/>.
    /// </summary>
    public static bool TryRestoreDefaults(Window window, out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!IsSupported)
        {
            error = "Native macOS window controls are only available on macOS.";
            return false;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.Handle == IntPtr.Zero)
        {
            error = "The window does not have a native handle yet.";
            return false;
        }

        try
        {
            if (!InitialStates.TryGetValue(window, out var state))
                state = CaptureState(handle.Handle);
            RestoreState(handle.Handle, state);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Gets the native standard-button state captured for the window without modifying it.
    /// </summary>
    public static bool TryGetDefaults(
        Window window,
        out MacOsWindowControlsOptions? options,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        options = null;
        if (!IsSupported)
        {
            error = "Native macOS window controls are only available on macOS.";
            return false;
        }

        var handle = window.TryGetPlatformHandle();
        if (handle is null || handle.Handle == IntPtr.Zero)
        {
            error = "The window does not have a native handle yet.";
            return false;
        }

        try
        {
            var state = InitialStates.GetValue(window, _ => CaptureState(handle.Handle));
            options = state.ToOptions();
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Applies the options directly to an NSWindow pointer.
    /// </summary>
    [SupportedOSPlatform("macos")]
    public static void Apply(IntPtr nsWindow, MacOsWindowControlsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (nsWindow == IntPtr.Zero)
            throw new ArgumentException("The NSWindow pointer cannot be zero.", nameof(nsWindow));

        var standardWindowButton = GetSelector("standardWindowButton:");
        foreach (var button in Enum.GetValues<MacOsWindowButton>())
        {
            var nativeButton = NativeMethods.SendIntPtr(nsWindow, standardWindowButton, (IntPtr)(int)button);
            if (nativeButton == IntPtr.Zero)
                continue;

            var (buttonOptions, position) = options.GetButton(button);
            NativeMethods.SendPoint(nativeButton, GetSelector("setFrameOrigin:"),
                new NativePoint(position.X, position.Y));

            if (buttonOptions.Size is { } size)
                NativeMethods.SendSize(nativeButton, GetSelector("setFrameSize:"),
                    new NativeSize(size.Width, size.Height));

            NativeMethods.SendBool(nativeButton, GetSelector("setHidden:"), !buttonOptions.IsVisible);
            NativeMethods.SendBool(nativeButton, GetSelector("setEnabled:"), buttonOptions.IsEnabled);
            NativeMethods.SendDouble(nativeButton, GetSelector("setAlphaValue:"),
                Math.Clamp(buttonOptions.Opacity, 0, 1));
        }
    }

    [SupportedOSPlatform("macos")]
    private static NativeWindowState CaptureState(IntPtr nsWindow)
    {
        var standardWindowButton = GetSelector("standardWindowButton:");
        var states = new NativeButtonState[3];
        foreach (var button in Enum.GetValues<MacOsWindowButton>())
        {
            var nativeButton = NativeMethods.SendIntPtr(nsWindow, standardWindowButton, (IntPtr)(int)button);
            if (nativeButton == IntPtr.Zero)
                continue;

            states[(int)button] = new NativeButtonState(
                GetFrame(nativeButton, GetSelector("frame")),
                NativeMethods.SendBoolResult(nativeButton, GetSelector("isHidden")) != 0,
                NativeMethods.SendBoolResult(nativeButton, GetSelector("isEnabled")) != 0,
                NativeMethods.SendDoubleResult(nativeButton, GetSelector("alphaValue")));
        }

        return new NativeWindowState(states);
    }

    [SupportedOSPlatform("macos")]
    private static void RestoreState(IntPtr nsWindow, NativeWindowState state)
    {
        var standardWindowButton = GetSelector("standardWindowButton:");
        foreach (var button in Enum.GetValues<MacOsWindowButton>())
        {
            var nativeButton = NativeMethods.SendIntPtr(nsWindow, standardWindowButton, (IntPtr)(int)button);
            if (nativeButton == IntPtr.Zero)
                continue;

            var buttonState = state.Buttons[(int)button];
            NativeMethods.SendRect(nativeButton, GetSelector("setFrame:"), buttonState.Frame);
            NativeMethods.SendBool(nativeButton, GetSelector("setHidden:"), buttonState.IsHidden);
            NativeMethods.SendBool(nativeButton, GetSelector("setEnabled:"), buttonState.IsEnabled);
            NativeMethods.SendDouble(nativeButton, GetSelector("setAlphaValue:"), buttonState.Opacity);
        }
    }

    [SupportedOSPlatform("macos")]
    private static NativeRect GetFrame(IntPtr nativeButton, IntPtr selector)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            NativeMethods.SendRectResultX64(out var result, nativeButton, selector);
            return result;
        }

        return NativeMethods.SendRectResult(nativeButton, selector);
    }

    [SupportedOSPlatform("macos")]
    private static IntPtr GetSelector(string name)
    {
        var selector = NativeMethods.RegisterSelector(name);
        return selector != IntPtr.Zero
            ? selector
            : throw new InvalidOperationException($"Could not resolve the Objective-C selector '{name}'.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct NativePoint(double X, double Y);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct NativeSize(double Width, double Height);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct NativeRect(NativePoint Origin, NativeSize Size);

    private sealed record NativeWindowState(NativeButtonState[] Buttons)
    {
        public MacOsWindowControlsOptions ToOptions() => new()
        {
            Close = Buttons[(int)MacOsWindowButton.Close].ToOptions(),
            Minimize = Buttons[(int)MacOsWindowButton.Minimize].ToOptions(),
            Zoom = Buttons[(int)MacOsWindowButton.Zoom].ToOptions()
        };
    }

    private readonly record struct NativeButtonState(NativeRect Frame, bool IsHidden, bool IsEnabled, double Opacity)
    {
        public MacOsWindowButtonOptions ToOptions() => new()
        {
            Position = new Point(Frame.Origin.X, Frame.Origin.Y),
            Size = new Size(Frame.Size.Width, Frame.Size.Height),
            IsVisible = !IsHidden,
            IsEnabled = IsEnabled,
            Opacity = Opacity
        };
    }

    [SupportedOSPlatform("macos")]
    private static class NativeMethods
    {
        private const string ObjectiveCRuntime = "/usr/lib/libobjc.A.dylib";

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern void SendPoint(IntPtr receiver, IntPtr selector, NativePoint argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern void SendSize(IntPtr receiver, IntPtr selector, NativeSize argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern void SendRect(IntPtr receiver, IntPtr selector, NativeRect argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern void SendBool(
            IntPtr receiver,
            IntPtr selector,
            [MarshalAs(UnmanagedType.I1)] bool argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern void SendDouble(IntPtr receiver, IntPtr selector, double argument);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern byte SendBoolResult(IntPtr receiver, IntPtr selector);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern double SendDoubleResult(IntPtr receiver, IntPtr selector);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend")]
        internal static extern NativeRect SendRectResult(IntPtr receiver, IntPtr selector);

        [DllImport(ObjectiveCRuntime, EntryPoint = "objc_msgSend_stret")]
        internal static extern void SendRectResultX64(out NativeRect result, IntPtr receiver, IntPtr selector);

        [DllImport(ObjectiveCRuntime, EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
        internal static extern IntPtr RegisterSelector(string name);
    }
}