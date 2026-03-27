using TurFirma.Data;
using TurFirma.Infrastructure;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly TourFirmaDbContext _db = new();
    private readonly AuthService _authService;
    private readonly TourService _tourService;
    private readonly BookingAppService _bookingService;
    private readonly ManagerService _managerService;

    public AuthViewModel Auth { get; }
    public TourCatalogViewModel Catalog { get; }
    public CartViewModel Cart { get; }
    public ProfileViewModel Profile { get; }
    public AdminViewModel Admin { get; }
    public RelayCommand NavigateCommand { get; }

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    private string _currentSection = "Auth";
    public string CurrentSection
    {
        get => _currentSection;
        set => SetProperty(ref _currentSection, value);
    }

    public MainViewModel()
    {
        _db.Database.EnsureCreated();
        _authService = new AuthService(_db);
        _tourService = new TourService(_db);
        _bookingService = new BookingAppService(_db);
        _managerService = new ManagerService(_db);

        Auth = new AuthViewModel(_authService);
        Catalog = new TourCatalogViewModel(_tourService, _bookingService, _authService);
        Cart = new CartViewModel(_bookingService, _authService);
        Profile = new ProfileViewModel(_bookingService, _authService);
        Admin = new AdminViewModel(_managerService, _authService, _db);
        NavigateCommand = new RelayCommand(section =>
        {
            if (section is string page && !string.IsNullOrWhiteSpace(page))
                CurrentSection = page;
        });

        Auth.LoggedIn += async (_, _) => await HandleLoggedInAsync();
        Catalog.BookingCreated += async (_, _) => await HandleBookingCreatedAsync();
        Cart.Paid += async (_, _) => await HandlePaidAsync();
    }

    private async Task HandleLoggedInAsync()
    {
        IsAuthenticated = true;
        CurrentSection = "Catalog";
        await Catalog.LoadToursAsync();
        await Cart.RefreshAsync();
        await Profile.LoadBookingsAsync();
        await Admin.LoadAsync();
    }

    private async Task HandleBookingCreatedAsync()
    {
        await Cart.RefreshAsync();
        await Profile.LoadBookingsAsync();
        await Admin.LoadAsync();
    }

    private async Task HandlePaidAsync()
    {
        await Cart.RefreshAsync();
        await Profile.LoadBookingsAsync();
        await Admin.LoadAsync();
    }
}
