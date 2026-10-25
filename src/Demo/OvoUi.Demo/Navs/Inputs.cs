using System.Collections.Generic;
using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Inputs
{
    public static readonly List<Page> InputsList =
    [
        Page.View<ComboBoxPage>("ComboBox"),
        Page.View<ColorPickerPage>("ColorPicker"),
        Page.View<TextBoxPage>("TextBox"),
        Page.View<AutoCompleteBoxPage>("AutoCompleteBox"),
        Page.View<OvoAutoCompleteBoxPage>("OvoAutoCompleteBox"),
        Page.View<ControlClassesInputPage>("ControlClassesInput"),
        Page.View<ClassSelectorPage>("ClassSelector"),
        Page.View<EnumSelectorPage>("EnumSelector"),
        Page.View<FormPage>("Form"),
        Page.View<MultiComboBoxPage>("MultiComboBox"),
        Page.View<NumPadPage>("NumPad"),
        Page.View<RatingPage>("Rating"),
        Page.View<SelectionListPage>("SelectionList"),
        Page.View<MultiAutoCompleteBoxPage>("MultiAutoCompleteBox"),
        Page.View<RangeSliderPage>("RangeSlider"),
        Page.View<TreeComboBoxPage>("TreeComboBox"),
        Page.View<KeyGestureInputPage>("KeyGestureInput"),
        Page.View<IPv4BoxPage>("IPv4Box"),
        Page.View<PathPickerPage>("PathPicker"),
        Page.View<PinCodePage>("PinCode"),
        Page.View<ButtonSpinnerPage>("ButtonSpinner"),
        Page.View<TagInputPage>("TagInput"),
        Page.View<NumericUpDownPage>("NumericUpDown"),
        Page.View<OvoNumericUpDownPage>("OvoNumericUpDown"),
        Page.View<SliderPage>("Slider"),
        Page.View<ManagedFileChooserPage>("ManagedFileChooser"),
    ];
}