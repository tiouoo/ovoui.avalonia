using OvoUi.Demo.Models;
using OvoUi.Demo.Views.Pages;

namespace OvoUi.Demo.Navs;

public static class Times
{
    public static readonly List<Page> TimesList =
    [
        new() { Title = "Calendar", Content = new BlankPage() },
        new() { Title = "CalendarDatePicker", Content = new BlankPage() },
        new() { Title = "Clock", Content = new BlankPage() },
        new() { Title = "DatePicker", Content = new BlankPage() },
        new() { Title = "DateTimePicker", Content = new BlankPage() },
        new() { Title = "TimeBox", Content = new BlankPage() },
        new() { Title = "TimePicker", Content = new BlankPage() },
    ];
}
