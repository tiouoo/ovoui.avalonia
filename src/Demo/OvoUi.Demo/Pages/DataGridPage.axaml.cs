using System.Collections.Generic;
using Avalonia.Controls;
namespace OvoUi.Demo.Pages;
public partial class DataGridPage : UserControl
{
 public DataGridPage() { InitializeComponent(); DataContext = this; }
 public IReadOnlyList<PersonRow> People { get; } = [new("Ada", "Designer", true), new("Linus", "Engineer", true), new("Grace", "Reviewer", false)];
 public string SourceCode { get; } = """
     public IReadOnlyList<PersonRow> People { get; } =
         [new("Ada", "Designer", true), new("Linus", "Engineer", true)];

     public record PersonRow(string Name, string Role, bool Active);
     """;
}
public record PersonRow(string Name, string Role, bool Active);

