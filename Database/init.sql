-- Tabloları sil
DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS Reviews;
DROP TABLE IF EXISTS Reservations;
DROP TABLE IF EXISTS TinyHouses;
DROP TABLE IF EXISTS Users;

-- Users tablosu
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Role INTEGER NOT NULL CHECK (Role IN (0, 1, 2)),
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- TinyHouses tablosu
CREATE TABLE TinyHouses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    Price DECIMAL NOT NULL,
    Location TEXT NOT NULL,
    SquareMeters INTEGER NOT NULL,
    OwnerId INTEGER NOT NULL,
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Reservations tablosu
CREATE TABLE Reservations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TinyHouseId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalPrice DECIMAL NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Beklemede',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TinyHouseId) REFERENCES TinyHouses(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Reviews tablosu
CREATE TABLE Reviews (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TinyHouseId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    Rating INTEGER NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (TinyHouseId) REFERENCES TinyHouses(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Payments tablosu
CREATE TABLE Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReservationId INTEGER NOT NULL,
    Amount DECIMAL NOT NULL,
    Status TEXT NOT NULL CHECK (Status IN ('Beklemede', 'Tamamlandı', 'İptalEdildi')),
    PaymentMethod TEXT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ReservationId) REFERENCES Reservations(Id) ON DELETE CASCADE
);

-- Trigger'ı düzelt (END; ile bitir)
DELIMITER //
CREATE TRIGGER IF NOT EXISTS CreatePaymentAfterReservation
AFTER INSERT ON Reservations
BEGIN
    INSERT INTO Payments (ReservationId, Amount, Status, PaymentMethod)
    VALUES (NEW.Id, NEW.TotalPrice, 'Beklemede', 'Kredi Kartı');
END;//
DELIMITER ;

-- Örnek admin kullanıcısı ekle
INSERT INTO Users (FirstName, LastName, Email, Password, Phone, Role, IsActive)
VALUES ('Admin', 'User', 'admin@tinyhouse.com', 'Admin123!', '5551234567', 0, 1);

-- SQLite'da foreign key kısıtlamalarını etkinleştir
PRAGMA foreign_keys = ON; 