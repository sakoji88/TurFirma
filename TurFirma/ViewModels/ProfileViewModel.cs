using System.Collections.ObjectModel;
using TurFirma.Infrastructure;
using TurFirma.Models;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class ProfileViewModel : ObservableObject
{
    private readonly BookingService _bookingService;
    private readonly AuthService _authService;

    public ObservableCollection<Booking> AllBookings { get; } = new();
    public RelayCommand LoadBookingsCommand { get; }

    public ProfileViewModel(BookingService bookingService, AuthService authService)
    {
        _bookingService = bookingService;
        _authService = authService;
        LoadBookingsCommand = new RelayCommand(async _ => await LoadAsync());
    }

    private async Task LoadAsync()
    {
        AllBookings.Clear();
        if (_authService.CurrentUser is null)
            return;

        foreach (var booking in await _bookingService.GetUserBookingsAsync(_authService.CurrentUser.Id))
            AllBookings.Add(booking);
    }
}
