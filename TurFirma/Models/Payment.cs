namespace TurFirma.Models;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; }
}
