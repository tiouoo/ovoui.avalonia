using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using OvoUi.Controls;
using OvoUi.Platform.Windows;

namespace OvoUi.Demo.Views;

public partial class CompositionMaterialWindow : OvoWindow
{
    private readonly List<WindowCompositionMaterial> _materials = [];
    private int _attachAttempt;

    public CompositionMaterialWindow()
    {
        InitializeComponent();
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TextOptions.SetTextHintingMode(this, TextHintingMode.Light);
        TextOptions.SetBaselinePixelAlignment(this, BaselinePixelAlignment.Aligned);
        Opened += (_, _) => Dispatcher.UIThread.Post(AttachMaterials, DispatcherPriority.Render);
        Closed += (_, _) => DisposeMaterials();
    }

    private void AttachMaterials()
    {
        DisposeMaterials();

        var acrylic = WindowCompositionMaterial.TryCreate(this, out var acrylicError);
        var mica = WindowCompositionMaterial.TryCreate(this, out var micaError);
        var acrylicReady = acrylic?.Apply(WindowCompositionMaterialKind.Acrylic,
            new Rect(60, 92, 600, 210), 18) == true;
        var micaReady = mica?.Apply(WindowCompositionMaterialKind.Mica,
            new Rect(60, 330, 600, 210), 18) == true;

        if (acrylicReady)
            _materials.Add(acrylic!);
        else
            acrylic?.Dispose();

        if (micaReady)
            _materials.Add(mica!);
        else
            mica?.Dispose();

        if (acrylicReady && micaReady)
            return;

        if (++_attachAttempt < 12)
            DispatcherTimer.RunOnce(AttachMaterials, TimeSpan.FromMilliseconds(50), DispatcherPriority.Render);
    }

    private void DisposeMaterials()
    {
        foreach (var material in _materials)
            material.Dispose();
        _materials.Clear();
    }
}