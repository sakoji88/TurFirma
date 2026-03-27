using Microsoft.EntityFrameworkCore;
using TurFirma.Data;
using TurFirma.Models;

namespace TurFirma.Services;

public class AuthService
{
    private readonly TourFirmaDbContext _db;

    public AuthService(TourFirmaDbContext db) => _db = db;

    public User? CurrentUser { get; private set; }

    public async Task<User> RegisterClientAsync(string fullName, string email, string phone, string passportSeries, string passportNumber, DateTime issueDate, string password)
    {
        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("Пользователь с таким email уже существует.");

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Phone = phone,
            PassportSeries = passportSeries,
            PassportNumber = passportNumber,
            PassportIssueDate = issueDate,
            PasswordHash = password,
            Role = UserRole.Client
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password)
            ?? throw new InvalidOperationException("Неверный логин или пароль.");

        CurrentUser = user;
        return user;
    }
}
