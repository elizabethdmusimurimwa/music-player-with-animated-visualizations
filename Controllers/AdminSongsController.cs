using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Helpers;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class AdminSongsController : AdminBaseController
    {
        // GET: AdminSongs?search=love&artistId=2&genreId=1&page=2
        public ActionResult Index(string search, int? artistId, int? genreId, int page = 1)
        {
            var songs = db.Songs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                songs = songs.Where(s => s.Title.Contains(term) ||
                                         s.Album.Contains(term) ||
                                         s.Artist.ArtistName.Contains(term));
            }
            if (artistId.HasValue)
            {
                int wantedArtist = artistId.Value;
                songs = songs.Where(s => s.ArtistId == wantedArtist);
            }
            if (genreId.HasValue)
            {
                int wantedGenre = genreId.Value;
                songs = songs.Where(s => s.GenreId == wantedGenre);
            }

            var rows = ToSongRows(songs);
            var pager = PagerInfo.Create(page, rows.Count(), PageSize);

            var model = new SongListViewModel
            {
                Songs = rows.OrderByDescending(r => r.SongId)
                            .Skip((pager.Page - 1) * PageSize)
                            .Take(PageSize)
                            .ToList(),
                Pager = pager,
                Search = search,
                ArtistId = artistId,
                GenreId = genreId,
                Artists = new SelectList(db.Artists.OrderBy(a => a.ArtistName).ToList(), "ArtistId", "ArtistName", artistId),
                Genres = new SelectList(db.Genres.OrderBy(g => g.GenreName).ToList(), "GenreId", "GenreName", genreId)
            };

            return View(model);
        }

        // ADD 

        // GET: AdminSongs/Create
        public ActionResult Create()
        {
            return FormView(new SongFormViewModel(), "Add a song");
        }

        // POST: AdminSongs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SongFormViewModel model)
        {
            CheckSong(model);
            if (!ModelState.IsValid)
            {
                return FormView(model, "Add a song");
            }

            Song song = new Song();
            CopyToSong(model, song);
            db.Songs.Add(song);
            db.SaveChanges();

            Success("\"" + song.Title + "\" was added.");
            return RedirectToAction("Index");
        }

        // EDIT 

        // GET: AdminSongs/Edit/5
        public ActionResult Edit(int id)
        {
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                Fail("That song no longer exists.");
                return RedirectToAction("Index");
            }

            var model = new SongFormViewModel
            {
                SongId = song.SongId,
                Title = song.Title,
                ArtistId = song.ArtistId,
                GenreId = song.GenreId,
                Album = song.Album,
                AudioPath = song.AudioPath,
                ImagePath = song.ImagePath,
                Duration = song.Duration > 0 ? (int?)song.Duration : null
            };
            return FormView(model, "Edit song");
        }

        // POST: AdminSongs/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SongFormViewModel model)
        {
            Song song = db.Songs.Find(model.SongId);
            if (song == null)
            {
                Fail("That song no longer exists.");
                return RedirectToAction("Index");
            }

            CheckSong(model);
            if (!ModelState.IsValid)
            {
                return FormView(model, "Edit song");
            }

            CopyToSong(model, song);
            db.SaveChanges();

            Success("\"" + song.Title + "\" was updated.");
            return RedirectToAction("Index");
        }

        // DELETE

        // POST: AdminSongs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                Fail("That song was already removed.");
                return RedirectToAction("Index");
            }

            // Take the song out of every playlist first (PlaylistSongs points at Songs)
            var links = db.PlaylistSongs.Where(p => p.SongId == id).ToList();
            db.PlaylistSongs.RemoveRange(links);

            db.Songs.Remove(song);
            db.SaveChanges();

            Success("\"" + song.Title + "\" was deleted.");
            return RedirectToAction("Index");
        }

        // helpers

        private ActionResult FormView(SongFormViewModel model, string heading)
        {
            model.Artists = new SelectList(db.Artists.OrderBy(a => a.ArtistName).ToList(), "ArtistId", "ArtistName", model.ArtistId);
            model.Genres = new SelectList(db.Genres.OrderBy(g => g.GenreName).ToList(), "GenreId", "GenreName", model.GenreId);
            ViewBag.Heading = heading;
            return View("Form", model);
        }

        // Checks the attributes can't express on their own
        private void CheckSong(SongFormViewModel model)
        {
            string title = (model.Title ?? "").Trim();

            if (model.ArtistId > 0 && !db.Artists.Any(a => a.ArtistId == model.ArtistId))
            {
                ModelState.AddModelError("ArtistId", "That artist doesn't exist.");
            }
            if (model.GenreId > 0 && !db.Genres.Any(g => g.GenreId == model.GenreId))
            {
                ModelState.AddModelError("GenreId", "That genre doesn't exist.");
            }

            // Same artist can't have two songs with the same title
            if (title != "" && model.ArtistId > 0 &&
                db.Songs.Any(s => s.Title == title && s.ArtistId == model.ArtistId && s.SongId != model.SongId))
            {
                ModelState.AddModelError("Title", "This artist already has a song with that title.");
            }

            // Audio: an uploaded file OR a path/URL is required
            if (FileUploadHelper.HasFile(model.AudioFile))
            {
                string problem = FileUploadHelper.Check(model.AudioFile, FileUploadHelper.AudioTypes, FileUploadHelper.MaxAudioMb);
                if (problem != "")
                {
                    ModelState.AddModelError("AudioFile", problem);
                }
            }
            else if (string.IsNullOrWhiteSpace(model.AudioPath))
            {
                ModelState.AddModelError("AudioPath", "Upload an audio file or enter a path or URL.");
            }

            // Cover image is optional, but if a file is chosen it must be valid
            if (FileUploadHelper.HasFile(model.ImageFile))
            {
                string problem = FileUploadHelper.Check(model.ImageFile, FileUploadHelper.ImageTypes, FileUploadHelper.MaxImageMb);
                if (problem != "")
                {
                    ModelState.AddModelError("ImageFile", problem);
                }
            }
        }

        // Copies the form values onto the Song (saving uploaded files on the way)
        private void CopyToSong(SongFormViewModel model, Song song)
        {
            song.Title = model.Title.Trim();
            song.ArtistId = model.ArtistId;
            song.GenreId = model.GenreId;
            song.Album = string.IsNullOrWhiteSpace(model.Album) ? null : model.Album.Trim();
            song.Duration = model.Duration ?? 0;

            if (FileUploadHelper.HasFile(model.AudioFile))
            {
                song.AudioPath = FileUploadHelper.Save(model.AudioFile, "~/Content/Music/");
            }
            else
            {
                song.AudioPath = model.AudioPath.Trim();
            }

            if (FileUploadHelper.HasFile(model.ImageFile))
            {
                song.ImagePath = FileUploadHelper.Save(model.ImageFile, "~/Content/Images/");
            }
            else
            {
                song.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();
            }
        }
    }
}
