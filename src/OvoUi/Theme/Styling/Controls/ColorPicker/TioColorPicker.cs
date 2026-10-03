using Avalonia;
using Avalonia.Controls;

namespace TioUi.Theme.Controls.ColorPicker;

/// <summary>
/// 扩展版 ColorPicker，支持设置默认选中的页面
/// </summary>
public class TioColorPicker : Avalonia.Controls.ColorPicker
{
    /// <summary>
    /// 定义 SelectedIndex 属性，用于设置默认选中的页面索引
    /// <para>0 = Spectrum（光谱）</para>
    /// <para>1 = Palette（调色板）</para>
    /// <para>2 = Components（组件）</para>
    /// </summary>
    public static new readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<TioColorPicker, int>(nameof(SelectedIndex), 0);

    /// <summary>
    /// 获取或设置默认选中的页面索引
    /// </summary>
    public new int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
}