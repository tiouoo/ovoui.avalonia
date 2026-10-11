using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OvoUi.Demo.Pages;

public partial class PopConfirmPage : Avalonia.Controls.UserControl
{
    public PopConfirmPage()
    {
        InitializeComponent();
        DataContext = new PopConfirmDemoViewModel();
    }
}

public partial class PopConfirmDemoViewModel : ObservableObject
{
    [ObservableProperty] private string _lastAction = "No action yet";

    public string SourceCode { get; } = """
                                        public partial class PopConfirmDemoViewModel : ObservableObject
                                        {
                                            [ObservableProperty]
                                            private string _lastAction = "No action yet";

                                            [RelayCommand]
                                            private void Delete() => LastAction = "Deployment deleted";

                                            [RelayCommand]
                                            private void Cancel() => LastAction = "Confirmation cancelled";

                                            [RelayCommand]
                                            private async Task AsyncConfirmAsync()
                                            {
                                                LastAction = "Operation running...";
                                                await Task.Delay(1500);
                                                LastAction = "Remote operation completed";
                                            }
                                        }
                                        """;

    [RelayCommand]
    private void Delete() => LastAction = "Deployment deleted";

    [RelayCommand]
    private void Publish() => LastAction = "Release published";

    [RelayCommand]
    private void Archive() => LastAction = "Item archived";

    [RelayCommand]
    private void Cancel() => LastAction = "Confirmation cancelled";

    [RelayCommand]
    private async Task AsyncConfirmAsync()
    {
        LastAction = "Operation running...";
        await Task.Delay(1500);
        LastAction = "Remote operation completed";
    }
}