using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace OvoUi.Controls;

[PseudoClasses(PC_Hidden, PC_Open, PC_Primary, PC_Secondary, PC_Close, PC_FullSize)]
[TemplatePart(PART_PrimaryButton, typeof(Button))]
[TemplatePart(PART_SecondaryButton, typeof(Button))]
[TemplatePart(PART_CloseButton, typeof(Button))]
public partial class ContentDialog
{
    public const string PART_PrimaryButton = "PART_PrimaryButton";
    public const string PART_SecondaryButton = "PART_SecondaryButton";
    public const string PART_CloseButton = "PART_CloseButton";
    public const string PC_Hidden = ":hidden";
    public const string PC_Open = ":open";
    public const string PC_Primary = ":primary";
    public const string PC_Secondary = ":secondary";
    public const string PC_Close = ":close";
    public const string PC_FullSize = ":fullsize";

    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<ContentDialog, object?>(nameof(Title));
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty = AvaloniaProperty.Register<ContentDialog, IDataTemplate?>(nameof(TitleTemplate));
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty = AvaloniaProperty.Register<ContentDialog, string?>(nameof(PrimaryButtonText));
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty = AvaloniaProperty.Register<ContentDialog, string?>(nameof(SecondaryButtonText));
    public static readonly StyledProperty<string?> CloseButtonTextProperty = AvaloniaProperty.Register<ContentDialog, string?>(nameof(CloseButtonText));
    public static readonly StyledProperty<ICommand?> PrimaryButtonCommandProperty = AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(PrimaryButtonCommand));
    public static readonly StyledProperty<object?> PrimaryButtonCommandParameterProperty = AvaloniaProperty.Register<ContentDialog, object?>(nameof(PrimaryButtonCommandParameter));
    public static readonly StyledProperty<ICommand?> SecondaryButtonCommandProperty = AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(SecondaryButtonCommand));
    public static readonly StyledProperty<object?> SecondaryButtonCommandParameterProperty = AvaloniaProperty.Register<ContentDialog, object?>(nameof(SecondaryButtonCommandParameter));
    public static readonly StyledProperty<ICommand?> CloseButtonCommandProperty = AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(CloseButtonCommand));
    public static readonly StyledProperty<object?> CloseButtonCommandParameterProperty = AvaloniaProperty.Register<ContentDialog, object?>(nameof(CloseButtonCommandParameter));
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);
    public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty = AvaloniaProperty.Register<ContentDialog, ContentDialogButton>(nameof(DefaultButton));
    public static readonly StyledProperty<bool> FullSizeDesiredProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(FullSizeDesired));
    public static readonly StyledProperty<bool> IsPrimaryButtonIconVisibleProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonIconVisible));
    public static readonly StyledProperty<bool> IsSecondaryButtonIconVisibleProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonIconVisible));
    public static readonly StyledProperty<bool> IsCloseButtonIconVisibleProperty = AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsCloseButtonIconVisible));
    public static readonly StyledProperty<Geometry?> PrimaryButtonIconProperty = AvaloniaProperty.Register<ContentDialog, Geometry?>(nameof(PrimaryButtonIcon));
    public static readonly StyledProperty<Geometry?> SecondaryButtonIconProperty = AvaloniaProperty.Register<ContentDialog, Geometry?>(nameof(SecondaryButtonIcon));
    public static readonly StyledProperty<Geometry?> CloseButtonIconProperty = AvaloniaProperty.Register<ContentDialog, Geometry?>(nameof(CloseButtonIcon));

    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public IDataTemplate? TitleTemplate { get => GetValue(TitleTemplateProperty); set => SetValue(TitleTemplateProperty, value); }
    public string? PrimaryButtonText { get => GetValue(PrimaryButtonTextProperty); set => SetValue(PrimaryButtonTextProperty, value); }
    public string? SecondaryButtonText { get => GetValue(SecondaryButtonTextProperty); set => SetValue(SecondaryButtonTextProperty, value); }
    public string? CloseButtonText { get => GetValue(CloseButtonTextProperty); set => SetValue(CloseButtonTextProperty, value); }
    public ICommand? PrimaryButtonCommand { get => GetValue(PrimaryButtonCommandProperty); set => SetValue(PrimaryButtonCommandProperty, value); }
    public object? PrimaryButtonCommandParameter { get => GetValue(PrimaryButtonCommandParameterProperty); set => SetValue(PrimaryButtonCommandParameterProperty, value); }
    public ICommand? SecondaryButtonCommand { get => GetValue(SecondaryButtonCommandProperty); set => SetValue(SecondaryButtonCommandProperty, value); }
    public object? SecondaryButtonCommandParameter { get => GetValue(SecondaryButtonCommandParameterProperty); set => SetValue(SecondaryButtonCommandParameterProperty, value); }
    public ICommand? CloseButtonCommand { get => GetValue(CloseButtonCommandProperty); set => SetValue(CloseButtonCommandProperty, value); }
    public object? CloseButtonCommandParameter { get => GetValue(CloseButtonCommandParameterProperty); set => SetValue(CloseButtonCommandParameterProperty, value); }
    public bool IsPrimaryButtonEnabled { get => GetValue(IsPrimaryButtonEnabledProperty); set => SetValue(IsPrimaryButtonEnabledProperty, value); }
    public bool IsSecondaryButtonEnabled { get => GetValue(IsSecondaryButtonEnabledProperty); set => SetValue(IsSecondaryButtonEnabledProperty, value); }
    public ContentDialogButton DefaultButton { get => GetValue(DefaultButtonProperty); set => SetValue(DefaultButtonProperty, value); }
    public bool FullSizeDesired { get => GetValue(FullSizeDesiredProperty); set => SetValue(FullSizeDesiredProperty, value); }
    public bool IsPrimaryButtonIconVisible { get => GetValue(IsPrimaryButtonIconVisibleProperty); set => SetValue(IsPrimaryButtonIconVisibleProperty, value); }
    public bool IsSecondaryButtonIconVisible { get => GetValue(IsSecondaryButtonIconVisibleProperty); set => SetValue(IsSecondaryButtonIconVisibleProperty, value); }
    public bool IsCloseButtonIconVisible { get => GetValue(IsCloseButtonIconVisibleProperty); set => SetValue(IsCloseButtonIconVisibleProperty, value); }
    public Geometry? PrimaryButtonIcon { get => GetValue(PrimaryButtonIconProperty); set => SetValue(PrimaryButtonIconProperty, value); }
    public Geometry? SecondaryButtonIcon { get => GetValue(SecondaryButtonIconProperty); set => SetValue(SecondaryButtonIconProperty, value); }
    public Geometry? CloseButtonIcon { get => GetValue(CloseButtonIconProperty); set => SetValue(CloseButtonIconProperty, value); }

    public event EventHandler? Opening;
    public event EventHandler? Opened;
    public event EventHandler<ContentDialogClosingEventArgs>? Closing;
    public event EventHandler<ContentDialogClosedEventArgs>? Closed;
    public event EventHandler<ContentDialogButtonClickEventArgs>? PrimaryButtonClick;
    public event EventHandler<ContentDialogButtonClickEventArgs>? SecondaryButtonClick;
    public event EventHandler<ContentDialogButtonClickEventArgs>? CloseButtonClick;
}
