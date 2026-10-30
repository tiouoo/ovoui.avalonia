using System.Runtime.InteropServices;
using Avalonia.Controls;
using OvoUi.Platform;
using OvoUi.Platform.MacOS;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo;

internal static class PlatformSmokeTest
{
    // Exercise native calls from the published AOT binary, including the reverse P/Invoke callback.
    public static async Task RunAsync(Window window, List<string> report)
    {
        MemoryOptimization.ForceGarbageCollection();
        report.Add("PASS Platform forced GC");
        if (OperatingSystem.IsWindows())
        {
            if (!WindowsAppIdentity.TrySetCurrentProcessAppId("cc.tiouo.OvoUi.Demo.AotSmokeTest", out var identityError))
                throw new InvalidOperationException(identityError);
            report.Add("PASS Platform application identity");
            if (!MemoryOptimization.TryTrimWorkingSet(out var memoryError))
                throw new InvalidOperationException(memoryError);
            report.Add("PASS Platform working set trim");

            var invoked = false;
            if (!WindowsSystemMenu.TryAddItem(window, "AOT smoke test", () => invoked = true, out var item, out var menuError))
                throw new InvalidOperationException(menuError);
            using (item)
            {
                SendMessage(window.TryGetPlatformHandle()!.Handle, 0x0112, (nint)item!.CommandId, 0);
                if (!invoked)
                    throw new InvalidOperationException("The native system menu callback was not invoked.");
                if (!item.Remove())
                    throw new InvalidOperationException("The system menu item was not removed.");
            }
            if (!WindowsSystemMenu.TryAddSeparator(window, out var separator, out menuError))
                throw new InvalidOperationException(menuError);
            using (separator)
            {
                if (!WindowsSystemMenu.TryClear(window, out menuError))
                    throw new InvalidOperationException(menuError);
            }
            report.Add("PASS Platform system menu add / callback / remove / clear");

            // Use only windows owned by this test; never reparent a user's window.
            var target = new Window { Title = "AOT hosting target", Width = 320, Height = 200 };
            var content = new Window { Title = "AOT hosted content", Width = 160, Height = 100, ShowInTaskbar = false };
            try
            {
                target.Show();
                content.Show();
                await Task.Delay(100);
                var handle = target.TryGetPlatformHandle()!.Handle;
                var info = WindowContentInjection.GetAvailableWindows().Single(candidate => candidate.Handle == handle);
                if (!WindowContentInjection.TryInject(content, info, out var session, out var error,
                        new WindowContentInjectionOptions { SynchronizeSize = false }))
                    throw new InvalidOperationException(error);
                using (session)
                {
                    session!.Synchronize();
                    if (GetParent(content.TryGetPlatformHandle()!.Handle) != handle)
                        throw new InvalidOperationException("The hosted window did not attach to its target.");
                }
                if (GetParent(content.TryGetPlatformHandle()!.Handle) != 0)
                    throw new InvalidOperationException("The hosted window parent was not restored.");
                report.Add("PASS Platform enumerate / host / synchronize / restore windows");
            }
            finally
            {
                content.Close();
                target.Close();
            }
        }

        if (OperatingSystem.IsMacOS())
        {
            if (!MacOsWindowControls.TryGetDefaults(window, out var options, out var error) ||
                !MacOsWindowControls.TryApply(window, options!, out error) ||
                !MacOsWindowControls.TryRestoreDefaults(window, out error))
                throw new InvalidOperationException(error);
            report.Add("PASS Platform macOS capture / apply / restore controls");
        }
        else if (MacOsWindowControls.TryGetDefaults(window, out _, out _))
        {
            throw new InvalidOperationException("The macOS platform guard accepted another operating system.");
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    private static extern nint GetParent(nint window);
}
