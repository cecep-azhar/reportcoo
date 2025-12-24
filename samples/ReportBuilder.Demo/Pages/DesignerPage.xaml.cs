using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ReportBuilder.Demo.Pages;

public sealed partial class DesignerPage : Page
{
    private static readonly string DatabasePath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
     "ReportBuilderDemo",
       "reportbuilder.db");

    public DesignerPage()
    {
        this.InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
     await Designer.InitializeAsync(DatabasePath);
    }
}
