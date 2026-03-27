using System.Collections.ObjectModel;
using TurFirma.Infrastructure;
using TurFirma.Models;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class CartViewModel : ObservableObject
{
    private readonly BookingAppService _bookingService;
    private readonly AuthService _authService;
    public event EventHandler? Paid;

    public ObservableCollection<Booking> NewBookings { get; } = new();
    public Booking? SelectedBooking { get; set; }
    public PaymentMethod SelectedMethod { get; set; } = PaymentMethod.Card;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand PayCommand { get; }

    private string _status = "Загрузите корзину";
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public CartViewModel(BookingAppService bookingService, AuthService authService)
    {
        _bookingService = bookingService;
        _authService = authService;
        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        PayCommand = new RelayCommand(async _ => await PayAsync());
    }

    public async Task RefreshAsync()
    {
        NewBookings.Clear();
        if (_authService.CurrentUser is null)
            return;

        foreach (var booking in await _bookingService.GetUserBookingsAsync(_authService.CurrentUser.Id))
            if (booking.Status == BookingStatus.New)
                NewBookings.Add(booking);

        Status = $"В корзине: {NewBookings.Count}";
    }

    private async Task PayAsync()
    {
        try
        {
            if (SelectedBooking is null)
                throw new InvalidOperationException("Выберите бронь для оплаты.");

            await _bookingService.ProcessPaymentAsync(SelectedBooking.Id, SelectedMethod);
            Status = "Оплата успешна. Статус брони: оплачена";
            await RefreshAsync();
            Paid?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }
}
