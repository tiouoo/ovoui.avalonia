using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using OvoUi.Common.Classes;
using OvoUi.Common.Contracts;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

public class OvoAutoCompleteBox : Avalonia.Controls.AutoCompleteBox, IClearControl
{
    // ReSharper disable once InconsistentNaming
    private const string PART_TextBox = "PART_TextBox";
    private bool _closeBySelectionFlag;

    private TextBox? _textbox;
    private Popup? _popup;

    static OvoAutoCompleteBox()
    {
        MinimumPrefixLengthProperty.OverrideDefaultValue<OvoAutoCompleteBox>(0);
    }

    public OvoAutoCompleteBox()
    {
        AddHandler(PointerReleasedEvent, OnCurrentPointerReleased!, RoutingStrategies.Tunnel);
    }

    public void Clear()
    {
        // Note: this method only resets Text to null. 
        // By default, AutoCompleteBox will clear the SelectedItem when Text is set to null.
        // But user can use custom Predicate to control the behavior when Text is set to null.
        SetCurrentValue(TextProperty, null);
    }

    private void OnCurrentPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var source = (e.Source as Control).FindAncestorOfType<ListBoxItem>();
        if (source is not null)
        {
            _closeBySelectionFlag = true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textbox?.RemoveHandler(PointerPressedEvent, OnBoxPointerPressed);
        _textbox = e.NameScope.Find<TextBox>(PART_TextBox);
        _popup = e.NameScope.Find<Popup>(PartNames.PART_Popup);
        _textbox?.AddHandler(PointerPressedEvent, OnBoxPointerPressed, handledEventsToo: true);
        PseudoClasses.Set(PseudoClassName.PC_Empty, string.IsNullOrEmpty(Text));
    }

    private void OnBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Equals(sender, _textbox)
            && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            && IsDropDownOpen == false)
        {
            // Don't open the dropdown when the press originates from an interactive
            // control embedded in InnerLeftContent/InnerRightContent (e.g. a ComboBox),
            // so that control can handle the click without the dropdown competing with it.
            if (e.Source is Visual source && source.FindAncestorOfType<ComboBox>() is not null)
                return;

            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    protected void OnGotFocus(RoutedEventArgs e)
    {
        // If the focus is set by keyboard navigation, open the dropdown.
        if (!_closeBySelectionFlag && IsDropDownOpen == false)
        {
            SetCurrentValue(IsDropDownOpenProperty, true);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
        {
            var value = change.GetNewValue<string?>();
            PseudoClasses.Set(PseudoClassName.PC_Empty, string.IsNullOrEmpty(value));
        }

        if (change.Property == IsDropDownOpenProperty && change.GetNewValue<bool>() == false)
        {

        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        var newElement = e.NewFocusedElement;
        if (newElement is Visual v && _popup?.IsInsidePopup(v) == true)
        {
            return;
        }
        if (Equals(newElement, _textbox))
        {
            return;
        }
        SetCurrentValue(IsDropDownOpenProperty, false);
    }
}