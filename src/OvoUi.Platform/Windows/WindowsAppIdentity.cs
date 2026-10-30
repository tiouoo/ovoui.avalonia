using System;
using System.Runtime.InteropServices;

namespace OvoUi.Platform.Windows;

/// <summary>Sets the Windows taskbar application identity without activating COM objects.</summary>
public static partial class WindowsAppIdentity
{
    public static bool IsSupported => OperatingSystem.IsWindows();

    public static bool TrySetCurrentProcessAppId(string appId, out string? error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        if (!IsSupported)
        {
            error = "Windows application identity is only available on Windows.";
            return false;
        }

        var result = SetCurrentProcessExplicitAppUserModelID(appId);
        error = result < 0 ? Marshal.GetExceptionForHR(result)?.Message ?? $"HRESULT 0x{result:X8}" : null;
        return result >= 0;
    }

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SetCurrentProcessExplicitAppUserModelID(string appId);
}
