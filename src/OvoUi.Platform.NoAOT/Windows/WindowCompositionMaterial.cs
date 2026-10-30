// Portions derived from CompositionMaterial.Avalonia:
// https://github.com/HelloWRC/CompositionMaterial.Avalonia
// Copyright (c) 2026 HelloWRC, licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace OvoUi.Platform.Windows;

public enum WindowCompositionMaterialKind
{
    Acrylic,
    Mica
}

[RequiresDynamicCode("Composition materials use runtime generic COM proxy creation. Use OvoUi.Platform.NoAOT with the managed runtime.")]
[RequiresUnreferencedCode("Composition materials access private Avalonia.Win32 members by reflection.")]
public sealed class WindowCompositionMaterial : IDisposable
{
    private readonly TopLevel _topLevel;
    private readonly Assembly _win32Assembly;
    private readonly object _syncRoot;
    private readonly object _compositor;
    private readonly object _rootVisual;
    private readonly object _rootContainer;
    private readonly object _rootChildren;
    private readonly object _sprite;
    private readonly object _visual;
    private readonly object _clipGeometry;
    private readonly Type _visualType;
    private readonly Type _spriteVisualType;
    private readonly Type _visualCollectionType;
    private readonly Type _winUiUtilsType;
    private readonly HostBackdropLease? _hostBackdropLease;
    private object? _brush;
    private bool _disposed;

    private WindowCompositionMaterial(TopLevel topLevel, Assembly win32Assembly, object syncRoot, object compositor,
        object rootVisual, object rootContainer, object rootChildren, object sprite, object visual,
        object clipGeometry, Type visualType, Type spriteVisualType, Type visualCollectionType, Type winUiUtilsType)
    {
        _topLevel = topLevel;
        _win32Assembly = win32Assembly;
        _syncRoot = syncRoot;
        _compositor = compositor;
        _rootVisual = rootVisual;
        _rootContainer = rootContainer;
        _rootChildren = rootChildren;
        _sprite = sprite;
        _visual = visual;
        _clipGeometry = clipGeometry;
        _visualType = visualType;
        _spriteVisualType = spriteVisualType;
        _visualCollectionType = visualCollectionType;
        _winUiUtilsType = winUiUtilsType;

        var handle = topLevel.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) && handle != IntPtr.Zero)
            _hostBackdropLease = HostBackdropLease.Acquire(handle);
    }

    public static WindowCompositionMaterial? TryCreate(TopLevel topLevel, out string? error)
    {
        error = null;
        if (!OperatingSystem.IsWindows())
        {
            error = "Windows is required.";
            return null;
        }

        object? rootVisual = null;
        object? rootContainer = null;
        object? rootChildren = null;
        object? sprite = null;
        object? visual = null;
        object? clipGeometry = null;

        try
        {
            // Do not gate on AssemblyName.Version. Compatibility is determined from the members
            // exposed by the Avalonia.Win32 assembly that is actually loaded by the application.
            var win32Assembly = Assembly.Load("Avalonia.Win32");
            var platform = topLevel.PlatformImpl ??
                           throw new InvalidOperationException("Window platform is unavailable.");
            var surface = FindField(platform.GetType(), "_glSurface").GetValue(platform)
                          ?? throw new InvalidOperationException("Window composition surface is unavailable.");

            if (!IsType(surface, "Avalonia.Win32.WinRT.Composition.WinUiCompositedWindowSurface"))
                throw new NotSupportedException(
                    $"The current renderer exposes {surface.GetType().FullName}, not a WinUI composition surface.");

            var window = FindField(surface.GetType(), "_window").GetValue(surface)
                         ?? throw new InvalidOperationException("WinUI composited window is unavailable.");
            var target = FindField(window.GetType(), "_target").GetValue(window)
                         ?? throw new InvalidOperationException("Composition target is unavailable.");
            var shared = FindField(window.GetType(), "_shared").GetValue(window)
                         ?? throw new InvalidOperationException("Shared compositor is unavailable.");
            var swapchainVisual = FindField(window.GetType(), "_visual").GetValue(window)
                                  ?? throw new InvalidOperationException("Swapchain visual is unavailable.");

            var sharedType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.Composition.WinUiCompositionShared");
            var compositor = ReadProperty(shared, sharedType, "Compositor");
            var syncRoot = ReadProperty(shared, sharedType, "SyncRoot");

            var targetType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.ICompositionTarget");
            rootVisual = ReadProperty(target, targetType, "Root");
            var containerVisualType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.IContainerVisual");
            rootContainer = QueryInterface(rootVisual, containerVisualType);
            rootChildren = ReadProperty(rootContainer, containerVisualType, "Children");

            var compositorType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.ICompositor");
            sprite = Invoke(compositor, compositorType, "CreateSpriteVisual")
                     ?? throw new InvalidOperationException("Unable to create material sprite.");
            var visualType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.IVisual");
            visual = QueryInterface(sprite, visualType);
            var visualCollectionType = RequireType(win32Assembly, "Avalonia.Win32.WinRT.IVisualCollection");

            lock (syncRoot)
                Invoke(rootChildren, visualCollectionType, "InsertBelow", visual, swapchainVisual);

            var winUiUtilsType = RequireType(win32Assembly,
                "Avalonia.Win32.WinRT.Composition.WinUiCompositionUtils");
            var visualArray = Array.CreateInstance(visualType, 1);
            visualArray.SetValue(visual, 0);
            clipGeometry = InvokeStatic(winUiUtilsType, "ClipVisual", compositor, (float?)0, visualArray)
                           ?? throw new InvalidOperationException("Unable to clip material sprite.");

            return new WindowCompositionMaterial(topLevel, win32Assembly, syncRoot, compositor, rootVisual,
                rootContainer, rootChildren, sprite, visual, clipGeometry, visualType,
                RequireType(win32Assembly, "Avalonia.Win32.WinRT.ISpriteVisual"), visualCollectionType,
                winUiUtilsType);
        }
        catch (Exception exception)
        {
            error = Unwrap(exception).Message;
            DisposeProxy(clipGeometry);
            DisposeProxy(visual);
            DisposeProxy(sprite);
            DisposeProxy(rootChildren);
            DisposeProxy(rootContainer);
            DisposeProxy(rootVisual);
            return null;
        }
    }

    public bool Apply(WindowCompositionMaterialKind kind, Rect bounds, double cornerRadius)
    {
        if (_disposed) return false;

        object? brush = null;
        try
        {
            brush = kind == WindowCompositionMaterialKind.Mica ? CreateMicaBrush() : CreateAcrylicBrush();
            if (brush is null) return false;

            lock (_syncRoot)
            {
                Invoke(_sprite, _spriteVisualType, "SetBrush", brush);
                DisposeProxy(_brush);
                _brush = brush;
                brush = null;
                UpdateGeometryCore(bounds, cornerRadius);
                Invoke(_visual, _visualType, "SetOpacity", 1f);
                Invoke(_visual, _visualType, "SetIsVisible", 1);
            }

            return true;
        }
        catch
        {
            DisposeProxy(brush);
            return false;
        }
    }

    public bool UpdateGeometry(Rect bounds, double cornerRadius)
    {
        if (_disposed) return false;
        try
        {
            lock (_syncRoot)
                UpdateGeometryCore(bounds, cornerRadius);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Hide()
    {
        if (_disposed) return;
        try
        {
            lock (_syncRoot)
                Invoke(_visual, _visualType, "SetIsVisible", 0);
        }
        catch
        {
            // The composition target may already be shutting down.
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_syncRoot)
        {
            try
            {
                Invoke(_rootChildren, _visualCollectionType, "Remove", _visual);
            }
            catch
            {
                // The native visual tree can disappear before Window.Closed is raised.
            }
        }

        DisposeProxy(_brush);
        DisposeProxy(_clipGeometry);
        DisposeProxy(_visual);
        DisposeProxy(_sprite);
        DisposeProxy(_rootChildren);
        DisposeProxy(_rootContainer);
        DisposeProxy(_rootVisual);
        _hostBackdropLease?.Dispose();
    }

    private void UpdateGeometryCore(Rect bounds, double cornerRadius)
    {
        var scaling = _topLevel.RenderScaling;
        var size = new Vector2((float)(bounds.Width * scaling), (float)(bounds.Height * scaling));
        var offset = new Vector3((float)(bounds.X * scaling), (float)(bounds.Y * scaling), 0);
        var radius = new Vector2((float)(Math.Max(0, cornerRadius) * scaling));
        var geometryType = RequireType(_win32Assembly,
            "Avalonia.Win32.WinRT.ICompositionRoundedRectangleGeometry");

        Invoke(_visual, _visualType, "SetOffset", offset);
        Invoke(_visual, _visualType, "SetSize", size);
        Invoke(_clipGeometry, geometryType, "SetSize", size);
        Invoke(_clipGeometry, geometryType, "SetCornerRadius", radius);
    }

    private object? CreateAcrylicBrush() =>
        InvokeStatic(_winUiUtilsType, "CreateAcrylicBlurBackdropBrush", _compositor);

    private object? CreateMicaBrush()
    {
        var theme = _topLevel.ActualThemeVariant;
        var dark = theme == ThemeVariant.Dark || theme.InheritVariant == ThemeVariant.Dark;
        return InvokeStatic(_winUiUtilsType, "CreateMicaBackdropBrush", _compositor,
                   dark ? 32f : 242f, dark ? 0.8f : 0.6f)
               ?? CreateAcrylicBrush();
    }

    private static bool IsType(object value, string fullName)
    {
        for (var type = value.GetType(); type is not null; type = type.BaseType)
            if (type.FullName == fullName)
                return true;
        return value.GetType().GetInterfaces().Any(type => type.FullName == fullName);
    }

    private static object QueryInterface(object value, Type interfaceType)
    {
        var microComRuntime = Type.GetType("MicroCom.Runtime.MicroComRuntime, MicroCom.Runtime", true)!;
        var unknownType = Type.GetType("MicroCom.Runtime.IUnknown, MicroCom.Runtime", true)!;
        if (!unknownType.IsInstanceOfType(value))
            throw new InvalidOperationException($"{value.GetType()} is not a MicroCom proxy.");
        var method = microComRuntime.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate => candidate.Name == "QueryInterface" && candidate.IsGenericMethodDefinition);
        return method.MakeGenericMethod(interfaceType).Invoke(null, [value])
               ?? throw new InvalidOperationException($"QueryInterface({interfaceType.Name}) returned null.");
    }

    private static object ReadProperty(object value, Type contractType, string name) =>
        (value.GetType().GetProperty(name, AllMembers) ?? contractType.GetProperty(name, AllMembers))?.GetValue(value)
        ?? throw new MissingMemberException(value.GetType().FullName, name);

    private static object? Invoke(object target, Type contractType, string name, params object?[] arguments) =>
        FindMethod(target.GetType(), contractType, name, arguments.Length).Invoke(target, arguments);

    private static object? InvokeStatic(Type type, string name, params object?[] arguments) =>
        FindMethod(type, type, name, arguments.Length, true).Invoke(null, arguments);

    private static MethodInfo FindMethod(Type runtimeType, Type contractType, string name, int parameterCount,
        bool requireStatic = false)
    {
        var candidates = runtimeType.GetMethods(AllMembers).Concat(contractType.GetMethods(AllMembers));
        return candidates.FirstOrDefault(method => method.Name == name && method.IsStatic == requireStatic &&
                                                   method.GetParameters().Length == parameterCount)
               ?? throw new MissingMethodException(runtimeType.FullName, name);
    }

    private static Type RequireType(Assembly assembly, string name) => assembly.GetType(name, true)!;

    private static FieldInfo FindField(Type type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var field = current.GetField(name, AllMembers);
            if (field is not null) return field;
        }

        throw new MissingFieldException(type.FullName, name);
    }

    private static Exception Unwrap(Exception exception)
    {
        while (exception is TargetInvocationException { InnerException: not null })
            exception = exception.InnerException;
        return exception;
    }

    private static void DisposeProxy(object? value)
    {
        if (value is IDisposable disposable) disposable.Dispose();
    }

    private const BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Static |
                                            BindingFlags.Public | BindingFlags.NonPublic;

    private sealed class HostBackdropLease : IDisposable
    {
        private static readonly object Gate = new();
        private static readonly Dictionary<IntPtr, State> States = [];
        private readonly IntPtr _windowHandle;
        private bool _disposed;

        private HostBackdropLease(IntPtr windowHandle) => _windowHandle = windowHandle;

        public static HostBackdropLease? Acquire(IntPtr windowHandle)
        {
            lock (Gate)
            {
                if (States.TryGetValue(windowHandle, out var state))
                {
                    state.ReferenceCount++;
                    return new HostBackdropLease(windowHandle);
                }

                DwmGetWindowAttribute(windowHandle, DwmwaUseHostBackdropBrush, out var previous, sizeof(int));
                var enabled = 1;
                if (DwmSetWindowAttribute(windowHandle, DwmwaUseHostBackdropBrush, ref enabled, sizeof(int)) != 0)
                    return null;
                States.Add(windowHandle, new State(previous));
                return new HostBackdropLease(windowHandle);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (Gate)
            {
                if (!States.TryGetValue(_windowHandle, out var state) || --state.ReferenceCount > 0) return;
                var previous = state.PreviousValue;
                DwmSetWindowAttribute(_windowHandle, DwmwaUseHostBackdropBrush, ref previous, sizeof(int));
                States.Remove(_windowHandle);
            }
        }

        private sealed class State(int previousValue)
        {
            public int PreviousValue { get; } = previousValue;
            public int ReferenceCount { get; set; } = 1;
        }
    }

    private const int DwmwaUseHostBackdropBrush = 17;

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
