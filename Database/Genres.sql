USE GrooveyTunesDB;
GO

IF OBJECT_ID('dbo.Genres', 'U') IS NULL
BEGIN
    CREATE TABLE Genres
    (
        GenreId INT IDENTITY(1,1) PRIMARY KEY,
        GenreName NVARCHAR(40) NOT NULL
    );
END
GO

