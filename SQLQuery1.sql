
USE GrooveyTunesDB;
GO

-- 1. Artists Table
CREATE TABLE Artists
(
    ArtistId INT IDENTITY(1,1) PRIMARY KEY,
    ArtistName NVARCHAR(150) NOT NULL,
    ImagePath NVARCHAR(500) NULL,
    Biography NVARCHAR(MAX) NULL
);
GO

-- 2. Songs Table
CREATE TABLE Songs
(
    SongId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    ArtistId INT NOT NULL,
    GenreId INT NOT NULL DEFAULT 0,
    Album NVARCHAR(200) NULL,
    AudioPath NVARCHAR(500) NULL,
    ImagePath NVARCHAR(500) NULL,
    Duration INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Songs_Artists
    FOREIGN KEY (ArtistId)
    REFERENCES Artists(ArtistId)
);
GO

-- 3. Playlists Table
CREATE TABLE Playlists
(
    PlaylistId INT IDENTITY(1,1) PRIMARY KEY,
    PlaylistName NVARCHAR(150) NOT NULL,
    UserId INT NOT NULL DEFAULT 1,
    ImagePath NVARCHAR(500) NULL
);
GO

-- 4. PlaylistSongs Table
CREATE TABLE PlaylistSongs
(
    PlaylistSongId INT IDENTITY(1,1) PRIMARY KEY,
    PlaylistId INT NOT NULL,
    SongId INT NOT NULL,

    CONSTRAINT FK_PlaylistSongs_Playlists
    FOREIGN KEY (PlaylistId)
    REFERENCES Playlists(PlaylistId),

    CONSTRAINT FK_PlaylistSongs_Songs
    FOREIGN KEY (SongId)
    REFERENCES Songs(SongId),

    CONSTRAINT UQ_PlaylistSongs
    UNIQUE (PlaylistId, SongId)
);
GO
