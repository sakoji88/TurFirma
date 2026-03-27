using Microsoft.EntityFrameworkCore;
using TurFirma.Models;

namespace TurFirma.Data;

public class TourFirmaDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<Transport> Transports => Set<Transport>();
    public DbSet<AdditionalService> AdditionalServices => Set<AdditionalService>();
    public DbSet<BookingAdditionalService> BookingServices => Set<BookingAdditionalService>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TurFirmaDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Manager>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Клиент Тестов",
                Email = "client@tour.local",
                Phone = "+79990000000",
                PassportSeries = "4510",
                PassportNumber = "123456",
                PassportIssueDate = new DateTime(2018, 5, 5),
                PasswordHash = "12345",
                Role = UserRole.Client
            },
            new User
            {
                Id = 2,
                FullName = "Менеджер Анна",
                Email = "manager@tour.local",
                Phone = "+79990000001",
                PassportSeries = "4511",
                PassportNumber = "654321",
                PassportIssueDate = new DateTime(2017, 6, 10),
                PasswordHash = "12345",
                Role = UserRole.Manager
            });

        modelBuilder.Entity<Manager>().HasData(new Manager { Id = 1, UserId = 2 });

        modelBuilder.Entity<BookingAdditionalService>()
            .HasKey(x => new { x.BookingId, x.AdditionalServiceId });

        modelBuilder.Entity<BookingAdditionalService>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.BookingId);

        modelBuilder.Entity<BookingAdditionalService>()
            .HasOne(x => x.AdditionalService)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.AdditionalServiceId);

        modelBuilder.Entity<AdditionalService>().HasData(
            new AdditionalService { Id = 1, Name = "Страховка", IsInsurance = true, InsurancePercentMin = 5, InsurancePercentMax = 10 },
            new AdditionalService { Id = 2, Name = "Трансфер", IsTransfer = true, TransferPricePerKm = 2.5m });

        modelBuilder.Entity<Guide>().HasData(
            new Guide { Id = 1, FullName = "Иван Серов", IsActive = true },
            new Guide { Id = 2, FullName = "Мария Смирнова", IsActive = true });

        modelBuilder.Entity<Transport>().HasData(
            new Transport { Id = 1, Name = "Mercedes Sprinter", Capacity = 20 },
            new Transport { Id = 2, Name = "Neoplan Tourliner", Capacity = 50 });

        modelBuilder.Entity<Tour>().HasData(
            new Tour { Id = 1, Destination = "Турция, Анталия", TourType = "Пляжный", StartDate = new DateTime(2026, 6, 10), EndDate = new DateTime(2026, 6, 20), BasePrice = 65000, GroupSizeMax = 20, DistanceKm = 45 },
            new Tour { Id = 2, Destination = "Италия, Рим", TourType = "Экскурсионный", StartDate = new DateTime(2026, 7, 5), EndDate = new DateTime(2026, 7, 12), BasePrice = 82000, GroupSizeMax = 15, DistanceKm = 35 },
            new Tour { Id = 3, Destination = "Кыргызстан, горный лагерь", TourType = "Активный", StartDate = new DateTime(2026, 8, 1), EndDate = new DateTime(2026, 8, 10), BasePrice = 59000, GroupSizeMax = 12, DistanceKm = 80 });
    }
}
