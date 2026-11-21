INSERT INTO Artists (ArtistName) VALUES ('Ariana Grande');
INSERT INTO Genres (GenreName) VALUES ('Pop');

INSERT INTO Songs (Title, ArtistId, GenreId, Album, AudioPath, ImagePath, Duration)
VALUES ('Side To Side',
        (SELECT TOP 1 ArtistId FROM Artists WHERE ArtistName = 'Ariana Grande'),
        (SELECT TOP 1 GenreId  FROM Genres  WHERE GenreName  = 'Pop'),
        'Dangerous Woman',
        '~/Content/Music/side-to-side.mp3',
        NULL,
        226);