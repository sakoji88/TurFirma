namespace TurFirma.Models;

public class BookingService
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int AdditionalServiceId { get; set; }
    public AdditionalService AdditionalService { get; set; } = null!;
    public decimal CalculatedPrice { get; set; }
}
