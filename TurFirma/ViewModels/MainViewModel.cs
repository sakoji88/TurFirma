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

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
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

        Auth.LoggedIn += (_, _) =>
        {
            IsAuthenticated = true;
            Catalog.LoadToursCommand.Execute(null);
            Profile.LoadBookingsCommand.Execute(null);
            Admin.LoadCommand.Execute(null);
        };

        Catalog.BookingCreated += (_, _) =>
        {
            Cart.RefreshCommand.Execute(null);
            Profile.LoadBookingsCommand.Execute(null);
            Admin.LoadCommand.Execute(null);
        };

        Cart.Paid += (_, _) =>
        {
            Profile.LoadBookingsCommand.Execute(null);
            Admin.LoadCommand.Execute(null);
        };
    }
}
