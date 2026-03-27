namespace TurFirma.Models;

public class Tour
{
    public int Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string TourType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BasePrice { get; set; }
    public int GroupSizeMax { get; set; }
    public int DistanceKm { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
