using Microsoft.EntityFrameworkCore;
using TurFirma.Data;
using TurFirma.Models;

namespace TurFirma.Services;

public class ManagerService
{
    private readonly TourFirmaDbContext _db;

    public ManagerService(TourFirmaDbContext db) => _db = db;

    public async Task<List<Booking>> GetPendingBookingsAsync()
    {
        return await _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Tour)
            .Where(b => b.Status == BookingStatus.Paid || b.Status == BookingStatus.New)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<List<Guide>> GetGuidesAsync() => await _db.Guides.Where(g => g.IsActive).ToListAsync();
    public async Task<List<Transport>> GetTransportsAsync() => await _db.Transports.OrderBy(t => t.Capacity).ToListAsync();

    public async Task AssignAndConfirmAsync(int bookingId, int managerId, int guideId, int transportId)
    {
        var booking = await _db.Bookings.Include(b => b.Tour).FirstAsync(b => b.Id == bookingId);
        var guideActiveCount = await _db.Bookings.CountAsync(b => b.GuideId == guideId && (b.Status == BookingStatus.Paid || b.Status == BookingStatus.Confirmed));
        if (guideActiveCount >= 3)
            throw new InvalidOperationException("Нельзя назначить гида: уже 3 активных тура.");

        var transport = await _db.Transports.FirstAsync(t => t.Id == transportId);
        if (transport.Capacity < booking.Seats)
            throw new InvalidOperationException("Вместимость транспорта меньше размера группы.");

        booking.ManagerId = managerId;
        booking.GuideId = guideId;
        booking.TransportId = transportId;
        booking.Status = BookingStatus.Confirmed;

        await _db.SaveChangesAsync();
    }
}
