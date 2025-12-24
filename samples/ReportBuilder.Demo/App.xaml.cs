using Microsoft.UI.Xaml;

namespace ReportBuilder.Demo;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
  MainWindow = new MainWindow();
        ReportBuilder.WinUI.Controls.App.MainWindow = MainWindow;
  MainWindow.Activate();
    }
}
