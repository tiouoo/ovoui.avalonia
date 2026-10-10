using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace OvoUi.Controls;

public class OvoCalendarDatePicker : CalendarDatePicker
{
    private Calendar? _calendar;
    private IDisposable? _displayModeSubscription;
    private bool _keepOpenForViewNavigation;
    private bool _restoringDropDown;
    private CalendarMode _previousDisplayMode = CalendarMode.Month;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _displayModeSubscription?.Dispose();
        _calendar = e.NameScope.Find<Calendar>("PART_Calendar");
        _displayModeSubscription = _calendar?.GetObservable(Calendar.DisplayModeProperty)
            .Subscribe(OnCalendarDisplayModeChanged);

        base.OnApplyTemplate(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsDropDownOpenProperty &&
            !change.GetNewValue<bool>() &&
            !_restoringDropDown &&
            _calendar is { } calendar &&
            _keepOpenForViewNavigation)
        {
            var displayMode = calendar.DisplayMode;
            _keepOpenForViewNavigation = false;
            base.OnPropertyChanged(change);

            Dispatcher.UIThread.Post(() =>
            {
                _restoringDropDown = true;
                SetCurrentValue(IsDropDownOpenProperty, true);
                calendar.DisplayMode = displayMode;
                _restoringDropDown = false;
            });
            return;
        }

        base.OnPropertyChanged(change);
    }

    private void OnCalendarDisplayModeChanged(CalendarMode displayMode)
    {
        if (IsDropDownOpen &&
            (_previousDisplayMode == CalendarMode.Decade && displayMode == CalendarMode.Year ||
             _previousDisplayMode == CalendarMode.Year && displayMode == CalendarMode.Month))
            _keepOpenForViewNavigation = true;

        _previousDisplayMode = displayMode;
    }
}
