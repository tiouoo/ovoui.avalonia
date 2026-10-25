using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Threading;

namespace OvoUi.Platform.Windows;

public sealed record NativeWindowInfo(IntPtr Handle, string Title, string ProcessName, int ProcessId)
{
    public override string ToString() => $"{Title} - {ProcessName} ({ProcessId})";
}

public sealed class WindowContentInjectionOptions
{
    public bool IsInteractive { get; init; } = true;
    public bool SynchronizeSize { get; init; } = true;
    public TimeSpan SynchronizationInterval { get; init; } = TimeSpan.FromMilliseconds(33);
}

public static class WindowContentInjection
{
    public static IReadOnlyList<NativeWindowInfo> GetAvailableWindows()
    {
        if (!OperatingSystem.IsWindows()) return [];

        var windows = new List<NativeWindowInfo>();
        NativeMethods.EnumWindows((handle, _) =>
        {
            if (!NativeMethods.IsWindowVisible(handle) ||
                NativeMethods.GetWindow(handle, NativeMethods.GwOwner) != IntPtr.Zero)
                return true;

            var length = NativeMethods.GetWindowTextLength(handle);
            if (length == 0) return true;

            var title = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(handle, title, title.Capacity);
            NativeMethods.GetWindowThreadProcessId(handle, out var processId);
            if (processId == 0) return true;

            string processName;
            try
            {
                using var process = Process.GetProcessById((int)processId);
                processName = process.ProcessName;
            }
            catch
            {
                processName = "Unknown";
            }

            windows.Add(new NativeWindowInfo(handle, title.ToString(), processName, (int)processId));
            return true;
        }, IntPtr.Zero);

        return windows.OrderBy(window => window.ProcessName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(window => window.Title, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static bool TryInject(Window contentWindow, NativeWindowInfo target,
        out WindowContentInjectionSession? session, out string? error,
        WindowContentInjectionOptions? options = null)
    {
        session = null;
        error = null;
        if (!OperatingSystem.IsWindows())
        {
            error = "Window content injection is only available on Windows.";
            return false;
        }

        var sourceHandle = contentWindow.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (sourceHandle == IntPtr.Zero)
        {
            error = "The content window must be shown before it can be injected.";
            return false;
        }

        if (target.Handle == IntPtr.Zero || !NativeMethods.IsWindow(target.Handle))
        {
            error = "The selected target window is no longer available.";
            return false;
        }

        if (sourceHandle == target.Handle)
        {
            error = "A window cannot be injected into itself.";
            return false;
        }

        try
        {
            session = new WindowContentInjectionSession(contentWindow, sourceHandle, target.Handle,
                options ?? new WindowContentInjectionOptions());
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }
}

public sealed class WindowContentInjectionSession : IDisposable
{
    private readonly Window _window;
    private readonly IntPtr _sourceHandle;
    private readonly IntPtr _originalParent;
    private readonly nint _originalStyle;
    private readonly nint _originalExtendedStyle;
    private readonly DispatcherTimer? _synchronizationTimer;
    private bool _disposed;

    internal WindowContentInjectionSession(Window window, IntPtr sourceHandle, IntPtr targetHandle,
        WindowContentInjectionOptions options)
    {
        _window = window;
        _sourceHandle = sourceHandle;
        TargetHandle = targetHandle;
        _originalParent = NativeMethods.GetParent(sourceHandle);
        _originalStyle = NativeMethods.GetWindowLongPtr(sourceHandle, NativeMethods.GwlStyle);
        _originalExtendedStyle = NativeMethods.GetWindowLongPtr(sourceHandle, NativeMethods.GwlExStyle);

        try
        {
            var style = _originalStyle | NativeMethods.WsChild;
            style &= ~(NativeMethods.WsCaption | NativeMethods.WsThickFrame | NativeMethods.WsPopup);
            var extendedStyle = _originalExtendedStyle | NativeMethods.WsExToolWindow;
            extendedStyle &= ~NativeMethods.WsExAppWindow;
            if (!options.IsInteractive)
                extendedStyle |= NativeMethods.WsExTransparent | NativeMethods.WsExNoActivate;

            NativeMethods.SetLastError(0);
            NativeMethods.SetWindowLongPtr(sourceHandle, NativeMethods.GwlStyle, style);
            ThrowLastErrorIfSet("Unable to update the content window style.");
            NativeMethods.SetLastError(0);
            NativeMethods.SetWindowLongPtr(sourceHandle, NativeMethods.GwlExStyle, extendedStyle);
            ThrowLastErrorIfSet("Unable to update the content window extended style.");

            NativeMethods.SetLastError(0);
            var previousParent = NativeMethods.SetParent(sourceHandle, targetHandle);
            if (previousParent == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "Unable to attach the content window. The target may be elevated or use an incompatible window host.");
        }
        catch
        {
            RestoreNativeWindow();
            throw;
        }

        NativeMethods.SetWindowPos(sourceHandle, IntPtr.Zero, 0, 0, 0, 0,
            NativeMethods.SwpFrameChanged | NativeMethods.SwpNoMove | NativeMethods.SwpNoSize |
            NativeMethods.SwpNoZOrder | NativeMethods.SwpShowWindow);
        Synchronize();

        if (options.SynchronizeSize)
        {
            _synchronizationTimer = new DispatcherTimer { Interval = options.SynchronizationInterval };
            _synchronizationTimer.Tick += SynchronizationTimerOnTick;
            _synchronizationTimer.Start();
        }

        _window.Closed += WindowOnClosed;
    }

    public IntPtr TargetHandle { get; }
    public bool IsDisposed => _disposed;

    public void Synchronize()
    {
        if (_disposed || !NativeMethods.IsWindow(TargetHandle) || !NativeMethods.IsWindow(_sourceHandle)) return;
        var clientRect = new NativeMethods.Rect();
        if (!NativeMethods.GetClientRect(TargetHandle, ref clientRect)) return;
        NativeMethods.SetWindowPos(_sourceHandle, IntPtr.Zero, 0, 0,
            Math.Max(1, clientRect.Right - clientRect.Left), Math.Max(1, clientRect.Bottom - clientRect.Top),
            NativeMethods.SwpNoActivate | NativeMethods.SwpNoZOrder | NativeMethods.SwpShowWindow);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _synchronizationTimer?.Stop();
        _window.Closed -= WindowOnClosed;
        if (!NativeMethods.IsWindow(_sourceHandle)) return;

        RestoreNativeWindow();
    }

    private void SynchronizationTimerOnTick(object? sender, EventArgs e)
    {
        if (!NativeMethods.IsWindow(TargetHandle))
        {
            Dispose();
            _window.Close();
            return;
        }

        Synchronize();
    }

    private void WindowOnClosed(object? sender, EventArgs e) => Dispose();

    private void RestoreNativeWindow()
    {
        NativeMethods.SetParent(_sourceHandle, _originalParent);
        NativeMethods.SetWindowLongPtr(_sourceHandle, NativeMethods.GwlStyle, _originalStyle);
        NativeMethods.SetWindowLongPtr(_sourceHandle, NativeMethods.GwlExStyle, _originalExtendedStyle);
        NativeMethods.SetWindowPos(_sourceHandle, IntPtr.Zero, 0, 0, 0, 0,
            NativeMethods.SwpFrameChanged | NativeMethods.SwpNoMove | NativeMethods.SwpNoSize |
            NativeMethods.SwpNoZOrder | NativeMethods.SwpNoActivate);
    }

    private static void ThrowLastErrorIfSet(string message)
    {
        var error = Marshal.GetLastWin32Error();
        if (error != 0) throw new Win32Exception(error, message);
    }
}

internal static partial class NativeMethods
{
    internal const int GwlStyle = -16;
    internal const int GwlExStyle = -20;
    internal const int GwOwner = 4;
    internal const nint WsChild = 0x40000000;
    internal static readonly nint WsPopup = unchecked((nint)0x80000000);
    internal const nint WsCaption = 0x00C00000;
    internal const nint WsThickFrame = 0x00040000;
    internal const nint WsExTransparent = 0x00000020;
    internal const nint WsExToolWindow = 0x00000080;
    internal const nint WsExAppWindow = 0x00040000;
    internal const nint WsExNoActivate = 0x08000000;
    internal const uint SwpNoSize = 0x0001;
    internal const uint SwpNoMove = 0x0002;
    internal const uint SwpNoZOrder = 0x0004;
    internal const uint SwpNoActivate = 0x0010;
    internal const uint SwpFrameChanged = 0x0020;
    internal const uint SwpShowWindow = 0x0040;

    internal delegate bool EnumWindowsProc(IntPtr handle, IntPtr parameter);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsWindowVisible(IntPtr handle);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsWindow(IntPtr handle);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
    internal static partial int GetWindowTextLength(IntPtr handle);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
    internal static extern int GetWindowText(IntPtr handle, StringBuilder text, int maximumCount);

    [LibraryImport("user32.dll")]
    internal static partial uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

    [LibraryImport("user32.dll")]
    internal static partial IntPtr GetWindow(IntPtr handle, uint command);

    [LibraryImport("user32.dll")]
    internal static partial IntPtr GetParent(IntPtr handle);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial IntPtr SetParent(IntPtr child, IntPtr newParent);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetClientRect(IntPtr handle, ref Rect rectangle);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtrW")]
    internal static partial nint GetWindowLongPtr(IntPtr handle, int index);

    [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtrW")]
    internal static partial nint SetWindowLongPtr(IntPtr handle, int index, nint value);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPos(IntPtr handle, IntPtr insertAfter, int x, int y, int width, int height,
        uint flags);

    [LibraryImport("kernel32.dll")]
    internal static partial void SetLastError(uint errorCode);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }
}