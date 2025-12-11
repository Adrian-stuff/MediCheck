-- Create Database if it doesn't exist
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'MediCheck_Login')
BEGIN
    CREATE DATABASE MediCheck_Login;
END
GO

USE MediCheck_Login;
GO

-- Users Table
CREATE TABLE UsersTable (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    Username NVARCHAR(50) UNIQUE,
    Email NVARCHAR(100),
    PasswordHash NVARCHAR(255),
    DateRegistered DATETIME DEFAULT GETDATE()
);

-- Medications Table
CREATE TABLE Medications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Dosage NVARCHAR(50),
    Type NVARCHAR(50),
    Notes NVARCHAR(MAX),
    StartDate DATE,
    EndDate DATE,
    Frequency NVARCHAR(50),
    TimeToTake TIME, -- Or DATETIME depending on preference, logic in C# handles both
    Stock INT,
    AlertLevel INT,
    UserID INT FOREIGN KEY REFERENCES UsersTable(UserID)
);

-- Medications Taken Table
CREATE TABLE MedicationsTaken (
    TakenID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT FOREIGN KEY REFERENCES UsersTable(UserID),
    MedicationID INT FOREIGN KEY REFERENCES Medications(Id),
    DateTaken DATETIME DEFAULT GETDATE()
);

-- Mock Data: Users
INSERT INTO UsersTable (FirstName, LastName, Username, Email, PasswordHash)
VALUES 
('Adrian', 'User', 'adrian', 'adrian@example.com', 'hash123'),
('Test', 'User', 'test', 'test@example.com', 'hash456');

-- Mock Data: Medications
-- Assuming UserID 1 is the logged in user
INSERT INTO Medications (Name, Dosage, Type, Notes, StartDate, EndDate, Frequency, TimeToTake, Stock, AlertLevel, UserID)
VALUES 
('Amoxicillin', '500mg', 'Antibiotic', 'Take with food', GETDATE(), DATEADD(day, 7, GETDATE()), 'Daily', '08:00:00', 20, 5, 1),
('Bioflu', '1 tab', 'Analgesic', 'For flu', GETDATE(), DATEADD(day, 5, GETDATE()), 'As needed', '13:00:00', 10, 2, 1),
('Vitamin C', '1000mg', 'Supplement', 'Daily boost', GETDATE(), DATEADD(year, 1, GETDATE()), 'Daily', '20:00:00', 50, 10, 1);

-- Mock Data: MedicationsTaken
-- Mark Amoxicillin (Id 1) as taken today
INSERT INTO MedicationsTaken (UserID, MedicationID, DateTaken)
VALUES (1, 1, GETDATE());
