namespace OvoUi.Common.Interfaces;

public interface IDialogContext
{
    public void Close();
    public event EventHandler<object?>? RequestClose;
}