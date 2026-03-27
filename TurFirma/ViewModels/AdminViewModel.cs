using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using TurFirma.Data;
using TurFirma.Infrastructure;
using TurFirma.Models;
using TurFirma.Services;

namespace TurFirma.ViewModels;

public class AdminViewModel : ObservableObject
{
    private readonly ManagerService _managerService;
    private readonly AuthService _authService;
    private readonly TourFirmaDbContext _db;

    public ObservableCollection<Booking> PendingBookings { get; } = new();
    public ObservableCollection<Guide> Guides { get; } = new();
    public ObservableCollection<Transport> Transports { get; } = new();

    public Booking? SelectedBooking { get; set; }
    public Guide? SelectedGuide { get; set; }
    public Transport? SelectedTransport { get; set; }

    public RelayCommand LoadCommand { get; }
    public RelayCommand ConfirmCommand { get; }

    private string _status = "Панель менеджера";
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public AdminViewModel(ManagerService managerService, AuthService authService, TourFirmaDbContext db)
    {
        _managerService = managerService;
        _authService = authService;
        _db = db;
        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        ConfirmCommand = new RelayCommand(async _ => await ConfirmAsync());
    }

    private async Task LoadAsync()
    {
        PendingBookings.Clear();
        Guides.Clear();
        Transports.Clear();

        foreach (var booking in await _managerService.GetPendingBookingsAsync())
            PendingBookings.Add(booking);

        foreach (var guide in await _managerService.GetGuidesAsync())
            Guides.Add(guide);

        foreach (var transport in await _managerService.GetTransportsAsync())
            Transports.Add(transport);
    }

    private async Task ConfirmAsync()
    {
        try
        {
            if (_authService.CurrentUser is null)
                throw new InvalidOperationException("Нужна авторизация менеджера");

            if (SelectedBooking is null || SelectedGuide is null || SelectedTransport is null)
                throw new InvalidOperationException("Выберите бронь, гида и транспорт.");

            var manager = await _db.Managers.FirstOrDefaultAsync(m => m.UserId == _authService.CurrentUser.Id);
            if (manager is null)
                throw new InvalidOperationException("Текущий пользователь не является менеджером.");

            await _managerService.AssignAndConfirmAsync(SelectedBooking.Id, manager.Id, SelectedGuide.Id, SelectedTransport.Id);
            Status = "Бронь подтверждена";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }
}
