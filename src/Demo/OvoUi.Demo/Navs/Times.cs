using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Times
{
    public static readonly List<Page> TimesList =
    [
        new() { Title = "Calendar" },
        new() { Title = "CalendarDatePicker" },
        new() { Title = "Clock" },
        new() { Title = "DatePicker" },
        new() { Title = "DateTimePicker" },
        new() { Title = "TimeBox" },
        new() { Title = "TimePicker" },
    ];
}
