using Microsoft.EntityFrameworkCore;
using TurFirma.Data;
using TurFirma.Models;

namespace TurFirma.Services;

public class TourService
{
    private readonly TourFirmaDbContext _db;
    public TourService(TourFirmaDbContext db) => _db = db;

    public async Task<List<Tour>> SearchToursAsync(string? destination, DateTime? fromDate, DateTime? toDate, decimal? maxPrice, string? type)
    {
        IQueryable<Tour> query = _db.Tours;

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.Destination.Contains(destination));

        if (fromDate.HasValue)
            query = query.Where(t => t.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.EndDate <= toDate.Value);

        if (maxPrice.HasValue)
            query = query.Where(t => t.BasePrice <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.TourType == type);

        return await query.OrderBy(t => t.StartDate).ToListAsync();
    }

    public async Task<int> GetAvailableSeatsAsync(int tourId)
    {
        var tour = await _db.Tours.FirstAsync(t => t.Id == tourId);
        var bookedSeats = await _db.Bookings
            .Where(b => b.TourId == tourId && (b.Status == BookingStatus.New || b.Status == BookingStatus.Paid || b.Status == BookingStatus.Confirmed))
            .SumAsync(x => x.Seats);

        return Math.Max(0, tour.GroupSizeMax - bookedSeats);
    }
}
