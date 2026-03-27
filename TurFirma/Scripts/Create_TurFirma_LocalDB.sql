IF DB_ID('TurFirmaDb') IS NULL
    CREATE DATABASE TurFirmaDb;
GO

USE TurFirmaDb;
GO

IF OBJECT_ID('dbo.BookingServices') IS NOT NULL DROP TABLE dbo.BookingServices;
IF OBJECT_ID('dbo.Payments') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Bookings') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.Managers') IS NOT NULL DROP TABLE dbo.Managers;
IF OBJECT_ID('dbo.Users') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Tours') IS NOT NULL DROP TABLE dbo.Tours;
IF OBJECT_ID('dbo.Guides') IS NOT NULL DROP TABLE dbo.Guides;
IF OBJECT_ID('dbo.Transport') IS NOT NULL DROP TABLE dbo.Transport;
IF OBJECT_ID('dbo.AdditionalServices') IS NOT NULL DROP TABLE dbo.AdditionalServices;
GO

CREATE TABLE dbo.Users (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    PassportSeries NVARCHAR(10) NOT NULL,
    PassportNumber NVARCHAR(20) NOT NULL,
    PassportIssueDate DATE NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Client','Manager'))
);

CREATE TABLE dbo.Managers (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    CONSTRAINT FK_Managers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);

CREATE TABLE dbo.Tours (
    Id INT IDENTITY PRIMARY KEY,
    Destination NVARCHAR(255) NOT NULL,
    TourType NVARCHAR(50) NOT NULL CHECK (TourType IN ('Пляжный','Экскурсионный','Активный')),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    BasePrice DECIMAL(12,2) NOT NULL CHECK (BasePrice > 0),
    GroupSizeMax INT NOT NULL CHECK (GroupSizeMax > 0),
    DistanceKm INT NOT NULL CHECK (DistanceKm >= 0),
    CONSTRAINT CK_Tours_Dates CHECK (StartDate < EndDate)
);

CREATE TABLE dbo.Guides (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Transport (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Capacity INT NOT NULL CHECK (Capacity > 0)
);

CREATE TABLE dbo.AdditionalServices (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    InsurancePercentMin DECIMAL(5,2) NULL CHECK (InsurancePercentMin >= 0),
    InsurancePercentMax DECIMAL(5,2) NULL CHECK (InsurancePercentMax >= 0),
    TransferPricePerKm DECIMAL(10,2) NULL CHECK (TransferPricePerKm >= 0),
    IsInsurance BIT NOT NULL DEFAULT 0,
    IsTransfer BIT NOT NULL DEFAULT 0,
    CONSTRAINT CK_AdditionalServices_Type CHECK (IsInsurance = 1 OR IsTransfer = 1)
);

CREATE TABLE dbo.Bookings (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    TourId INT NOT NULL,
    ManagerId INT NULL,
    GuideId INT NULL,
    TransportId INT NULL,
    Seats INT NOT NULL CHECK (Seats > 0),
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Новая','Оплачена','Подтверждена','Отменена','Истекла')),
    CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReservedUntilUtc DATETIME2 NOT NULL,
    TotalPrice DECIMAL(12,2) NOT NULL CHECK (TotalPrice >= 0),
    CONSTRAINT FK_Bookings_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Bookings_Tours FOREIGN KEY (TourId) REFERENCES dbo.Tours(Id),
    CONSTRAINT FK_Bookings_Managers FOREIGN KEY (ManagerId) REFERENCES dbo.Managers(Id),
    CONSTRAINT FK_Bookings_Guides FOREIGN KEY (GuideId) REFERENCES dbo.Guides(Id),
    CONSTRAINT FK_Bookings_Transport FOREIGN KEY (TransportId) REFERENCES dbo.Transport(Id)
);

CREATE TABLE dbo.Payments (
    Id INT IDENTITY PRIMARY KEY,
    BookingId INT NOT NULL,
    Amount DECIMAL(12,2) NOT NULL CHECK (Amount >= 0),
    Method NVARCHAR(20) NOT NULL CHECK (Method IN ('Card','EWallet')),
    PaidAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsSuccessful BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(Id)
);

CREATE TABLE dbo.BookingServices (
    BookingId INT NOT NULL,
    AdditionalServiceId INT NOT NULL,
    CalculatedPrice DECIMAL(12,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_BookingServices PRIMARY KEY (BookingId, AdditionalServiceId),
    CONSTRAINT FK_BookingServices_Bookings FOREIGN KEY (BookingId) REFERENCES dbo.Bookings(Id),
    CONSTRAINT FK_BookingServices_AdditionalServices FOREIGN KEY (AdditionalServiceId) REFERENCES dbo.AdditionalServices(Id)
);
GO

CREATE OR ALTER TRIGGER TR_Bookings_CheckGuideLoad
ON dbo.Bookings
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.GuideId IS NOT NULL
        AND (
            SELECT COUNT(*)
            FROM dbo.Bookings b
            WHERE b.GuideId = i.GuideId
              AND b.Status IN ('Оплачена','Подтверждена')
        ) > 3
    )
    BEGIN
        RAISERROR (N'Нарушение ограничения: один гид может иметь не более 3 активных туров.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

CREATE OR ALTER TRIGGER TR_Bookings_CheckTransportCapacity
ON dbo.Bookings
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Transport t ON t.Id = i.TransportId
        WHERE i.TransportId IS NOT NULL
          AND t.Capacity < i.Seats
    )
    BEGIN
        RAISERROR (N'Нарушение ограничения: вместимость транспорта меньше размера группы.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

INSERT INTO dbo.Users (FullName, Email, Phone, PassportSeries, PassportNumber, PassportIssueDate, PasswordHash, Role)
VALUES
(N'Клиент Тестов', N'client@tour.local', N'+79990000000', N'4510', N'123456', '2018-05-05', N'12345', N'Client'),
(N'Менеджер Анна', N'manager@tour.local', N'+79990000001', N'4511', N'654321', '2017-06-10', N'12345', N'Manager');

INSERT INTO dbo.Managers(UserId)
SELECT Id FROM dbo.Users WHERE Email = N'manager@tour.local';

INSERT INTO dbo.Guides (FullName, IsActive) VALUES
(N'Иван Серов', 1),
(N'Мария Смирнова', 1),
(N'Олег Дроздов', 1);

INSERT INTO dbo.Transport (Name, Capacity) VALUES
(N'Mercedes Sprinter', 20),
(N'Neoplan Tourliner', 50),
(N'Ford Transit', 12);

INSERT INTO dbo.AdditionalServices (Name, InsurancePercentMin, InsurancePercentMax, TransferPricePerKm, IsInsurance, IsTransfer)
VALUES
(N'Страховка', 5, 10, NULL, 1, 0),
(N'Трансфер', NULL, NULL, 2.50, 0, 1);

INSERT INTO dbo.Tours (Destination, TourType, StartDate, EndDate, BasePrice, GroupSizeMax, DistanceKm) VALUES
(N'Турция, Анталия', N'Пляжный', '2026-06-10', '2026-06-20', 65000, 20, 45),
(N'Италия, Рим', N'Экскурсионный', '2026-07-05', '2026-07-12', 82000, 15, 35),
(N'Кыргызстан, горный лагерь', N'Активный', '2026-08-01', '2026-08-10', 59000, 12, 80);
GO
