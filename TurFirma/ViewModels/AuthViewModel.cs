using TurFirma.Infrastructure;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class AuthViewModel : ObservableObject
{
    private readonly AuthService _authService;
    public event EventHandler? LoggedIn;

    public RelayCommand RegisterCommand { get; }
    public RelayCommand LoginCommand { get; }

    public string FullName { get; set; } = "Тестовый Клиент";
    public string Email { get; set; } = "client@tour.local";
    public string Phone { get; set; } = "+79990000000";
    public string PassportSeries { get; set; } = "4510";
    public string PassportNumber { get; set; } = "123456";
    public DateTime PassportIssueDate { get; set; } = new DateTime(2018, 5, 5);
    public string Password { get; set; } = "12345";

    private string _status = "Введите данные для входа или регистрации";
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;
        RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
    }

    private async Task RegisterAsync()
    {
        try
        {
            await _authService.RegisterClientAsync(FullName, Email, Phone, PassportSeries, PassportNumber, PassportIssueDate, Password);
            Status = "Регистрация успешна. Теперь выполните вход.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private async Task LoginAsync()
    {
        try
        {
            await _authService.LoginAsync(Email, Password);
            Status = "Вход выполнен";
            LoggedIn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }
}
