namespace TurFirma.Models;

public enum UserRole
{
    Client,
    Manager
}

public enum BookingStatus
{
    New,
    Paid,
    Confirmed,
    Cancelled,
    Expired
}

public enum PaymentMethod
{
    Card,
    EWallet
}
