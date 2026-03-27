namespace TurFirma.Models;

public class AdditionalService
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal InsurancePercentMin { get; set; }
    public decimal InsurancePercentMax { get; set; }
    public decimal TransferPricePerKm { get; set; }
    public bool IsInsurance { get; set; }
    public bool IsTransfer { get; set; }

    public ICollection<BookingAdditionalService> BookingServices { get; set; } = new List<BookingAdditionalService>();
}
