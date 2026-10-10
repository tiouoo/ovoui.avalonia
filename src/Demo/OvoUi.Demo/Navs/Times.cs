using OvoUi.Demo.Models;
using OvoUi.Demo.Pages;

namespace OvoUi.Demo.Navs;

public static class Times
{
    public static readonly List<Page> TimesList =
    [
        Page.View<CalendarPage>("Calendar"),
        Page.View<CalendarDatePickerPage>("CalendarDatePicker"),
        Page.View<DatePickerPage>("DatePicker"),
        Page.View<TimePickerPage>("TimePicker"),
        Page.View<ClockPage>("Clock"),
        Page.View<TimeBoxPage>("TimeBox")
    ];
}
