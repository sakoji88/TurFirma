namespace TurFirma.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PassportSeries { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime PassportIssueDate { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
