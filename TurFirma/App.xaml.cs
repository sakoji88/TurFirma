using System.Windows;
using TurFirma.ViewModels;

namespace TurFirma;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainViewModel = new MainViewModel();
        var loginWindow = new LoginWindow(mainViewModel);
        loginWindow.Show();
    }
}
