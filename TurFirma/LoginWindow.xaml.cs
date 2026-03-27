using System.Windows;
using TurFirma.ViewModels;

namespace TurFirma;

public partial class LoginWindow : Window
{
    private readonly MainViewModel _mainViewModel;

    public LoginWindow(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        DataContext = _mainViewModel;
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        _mainViewModel.Auth.Password = PasswordInput.Password;
        await _mainViewModel.Auth.LoginAsync();

        if (_mainViewModel.Auth.IsAuthenticated)
        {
            var mainWindow = new MainWindow(_mainViewModel);
            mainWindow.Show();
            Close();
        }
    }
}
