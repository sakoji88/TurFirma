using System.Collections.ObjectModel;
using TurFirma.Infrastructure;
using TurFirma.Models;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class TourCatalogViewModel : ObservableObject
{
    private readonly TourService _tourService;
    private readonly BookingAppService _bookingService;
    private readonly AuthService _authService;

    public ObservableCollection<Tour> Tours { get; } = new();
    public ObservableCollection<AdditionalService> Services { get; } = new();

    public RelayCommand LoadToursCommand { get; }
    public RelayCommand BookCommand { get; }
    public event EventHandler? BookingCreated;

    public string Destination { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MaxPrice { get; set; }
    public string TourType { get; set; } = string.Empty;
    public int Seats { get; set; } = 1;
    public Tour? SelectedTour { get; set; }
    public bool InsuranceSelected { get; set; }
    public bool TransferSelected { get; set; }

    private string _status = "Выберите тур";
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public TourCatalogViewModel(TourService tourService, BookingAppService bookingService, AuthService authService)
    {
        _tourService = tourService;
        _bookingService = bookingService;
        _authService = authService;

        LoadToursCommand = new RelayCommand(async _ => await LoadToursAsync());
        BookCommand = new RelayCommand(async parameter => await CreateBookingAsync(parameter));

        _ = LoadServicesAsync();
    }

    private async Task LoadServicesAsync()
    {
        Services.Clear();
        foreach (var service in await _bookingService.GetAdditionalServicesAsync())
            Services.Add(service);
    }

    private async Task LoadToursAsync()
    {
        Tours.Clear();
        foreach (var tour in await _tourService.SearchToursAsync(Destination, FromDate, ToDate, MaxPrice, TourType))
            Tours.Add(tour);

        Status = $"Найдено туров: {Tours.Count}";
    }

    private async Task CreateBookingAsync(object? parameter)
    {
        try
        {
            if (_authService.CurrentUser is null)
                throw new InvalidOperationException("Сначала войдите в систему.");

            if (parameter is Tour fromCard)
                SelectedTour = fromCard;

            if (SelectedTour is null)
                throw new InvalidOperationException("Выберите тур.");

            var selectedServiceIds = new List<int>();
            if (InsuranceSelected) selectedServiceIds.Add(1);
            if (TransferSelected) selectedServiceIds.Add(2);

            var booking = await _bookingService.CreateBookingAsync(_authService.CurrentUser.Id, SelectedTour.Id, Seats, selectedServiceIds);
            Status = $"Бронь #{booking.Id} создана. Резерв до {booking.ReservedUntilUtc:dd.MM.yyyy HH:mm} UTC";
            BookingCreated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }
}
