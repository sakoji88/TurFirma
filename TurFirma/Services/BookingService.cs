using Microsoft.EntityFrameworkCore;
using TurFirma.Data;
using TurFirma.Models;

namespace TurFirma.Services;

public class BookingService
{
    private readonly TourFirmaDbContext _db;
    private readonly Random _random = new();

    public BookingService(TourFirmaDbContext db) => _db = db;

    public async Task<List<AdditionalService>> GetAdditionalServicesAsync() => await _db.AdditionalServices.OrderBy(s => s.Id).ToListAsync();

    public async Task<decimal> CalculateTotalAsync(int tourId, int seats, IReadOnlyCollection<int> additionalServiceIds)
    {
        var tour = await _db.Tours.FirstAsync(x => x.Id == tourId);
        decimal total = tour.BasePrice * seats;

        foreach (var service in await _db.AdditionalServices.Where(s => additionalServiceIds.Contains(s.Id)).ToListAsync())
        {
            if (service.IsInsurance)
            {
                var insurancePercent = (decimal)_random.NextDouble() * (service.InsurancePercentMax - service.InsurancePercentMin) + service.InsurancePercentMin;
                total += total * insurancePercent / 100m;
            }

            if (service.IsTransfer)
                total += service.TransferPricePerKm * tour.DistanceKm;
        }

        return decimal.Round(total, 2);
    }

    public async Task<Booking> CreateBookingAsync(int userId, int tourId, int seats, IReadOnlyCollection<int> additionalServiceIds)
    {
        var available = await GetAvailableSeatsAsync(tourId);
        if (seats > available)
            throw new InvalidOperationException($"Недостаточно мест. Доступно: {available}");

        var total = await CalculateTotalAsync(tourId, seats, additionalServiceIds);
        var booking = new Booking
        {
            UserId = userId,
            TourId = tourId,
            Seats = seats,
            Status = BookingStatus.New,
            CreatedAtUtc = DateTime.UtcNow,
            ReservedUntilUtc = DateTime.UtcNow.AddHours(24),
            TotalPrice = total
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var services = await _db.AdditionalServices.Where(s => additionalServiceIds.Contains(s.Id)).ToListAsync();
        foreach (var service in services)
        {
            _db.BookingServices.Add(new BookingAdditionalService
            {
                BookingId = booking.Id,
                AdditionalServiceId = service.Id,
                CalculatedPrice = service.IsTransfer ? service.TransferPricePerKm : 0
            });
        }

        await _db.SaveChangesAsync();
        return booking;
    }

    public async Task ProcessPaymentAsync(int bookingId, PaymentMethod method)
    {
        var booking = await _db.Bookings.FirstAsync(x => x.Id == bookingId);
        if (booking.ReservedUntilUtc < DateTime.UtcNow && booking.Status == BookingStatus.New)
        {
            booking.Status = BookingStatus.Expired;
            await _db.SaveChangesAsync();
            throw new InvalidOperationException("Срок резерва истек.");
        }

        _db.Payments.Add(new Payment
        {
            BookingId = bookingId,
            Amount = booking.TotalPrice,
            Method = method,
            IsSuccessful = true,
            PaidAtUtc = DateTime.UtcNow
        });

        booking.Status = BookingStatus.Paid;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetAvailableSeatsAsync(int tourId)
    {
        var tour = await _db.Tours.FirstAsync(t => t.Id == tourId);
        var bookedSeats = await _db.Bookings.Where(x => x.TourId == tourId && x.Status != BookingStatus.Cancelled && x.Status != BookingStatus.Expired).SumAsync(x => x.Seats);
        return Math.Max(0, tour.GroupSizeMax - bookedSeats);
    }

    public async Task<List<Booking>> GetUserBookingsAsync(int userId)
    {
        return await _db.Bookings
            .Include(b => b.Tour)
            .Include(b => b.Payments)
            .Where(b => b.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }
}
