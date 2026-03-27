namespace TurFirma.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int TourId { get; set; }
    public Tour Tour { get; set; } = null!;
    public int? ManagerId { get; set; }
    public Manager? Manager { get; set; }
    public int? GuideId { get; set; }
    public Guide? Guide { get; set; }
    public int? TransportId { get; set; }
    public Transport? Transport { get; set; }
    public int Seats { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.New;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ReservedUntilUtc { get; set; }
    public decimal TotalPrice { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}
