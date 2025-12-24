using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReportBuilder.Demo.Pages;

namespace ReportBuilder.Demo;

public sealed partial class MainWindow : Window
{
 public MainWindow()
    {
        this.InitializeComponent();
 Title = "ReportBuilder Demo - Template Designer & Report Generator";
  
   // Navigate to default page
    ContentFrame.Navigate(typeof(DesignerPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
     {
   var tag = item.Tag?.ToString();
     switch (tag)
    {
      case "Designer":
         ContentFrame.Navigate(typeof(DesignerPage));
           break;
  case "Generate":
     ContentFrame.Navigate(typeof(GeneratePage));
  break;
         case "Demo":
            ContentFrame.Navigate(typeof(DemoPage));
         break;
     }
        }
 }
}
