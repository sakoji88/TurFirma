using TurFirma.Infrastructure;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class AuthViewModel : ObservableObject
{
    private readonly AuthService _authService;
    public event EventHandler? LoggedIn;

    public RelayCommand RegisterCommand { get; }
    public RelayCommand LoginCommand { get; }
    public bool IsAuthenticated { get; private set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PassportSeries { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime PassportIssueDate { get; set; } = DateTime.Today;
    public string Password { get; set; } = string.Empty;

    private string _status = string.Empty;
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;
        RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
    }

    public async Task RegisterAsync()
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

    public async Task LoginAsync()
    {
        try
        {
            await _authService.LoginAsync(Email, Password);
            Status = "Вход выполнен";
            IsAuthenticated = true;
            LoggedIn?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            IsAuthenticated = false;
            Status = ex.Message;
        }
    }
}
