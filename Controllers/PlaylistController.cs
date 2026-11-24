
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Playlist
        public ActionResult Index()
        {
            var playlists = db.Playlists
                .OrderBy(p => p.PlaylistName)
                .ToList();

            return View(playlists);
        }

        // GET: Playlist/Details/5
        public ActionResult Details(int id)
        {
            var playlist = db.Playlists
                .Include("PlaylistSongs.Song.Artist")
                .FirstOrDefault(p => p.PlaylistId == id);

            if (playlist == null)
                return HttpNotFound();

            return View(playlist);
        }

        // GET: Playlist/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Playlist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Playlist playlist)
        {
            if (ModelState.IsValid)
            {
                playlist.PlaylistName = playlist.PlaylistName.Trim();
                playlist.UserId = 1; // Temporary until user accounts are implemented

                db.Playlists.Add(playlist);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(playlist);
        }

        // GET: Playlist/AddSong/5
        public ActionResult AddSong(int id)
        {
            var playlist = db.Playlists.Find(id);

            if (playlist == null)
                return HttpNotFound();

            var existingSongIds = db.PlaylistSongs
                .Where(ps => ps.PlaylistId == id)
                .Select(ps => ps.SongId);

            var availableSongs = db.Songs
                .Include("Artist")
                .Where(s => !existingSongIds.Contains(s.SongId))
                .OrderBy(s => s.Title)
                .ToList();

            ViewBag.PlaylistId = id;
            ViewBag.PlaylistName = playlist.PlaylistName;

            return View(availableSongs);
        }

        // POST: Playlist/AddSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSong(int playlistId, int[] selectedSongIds)
        {
            var playlist = db.Playlists.Find(playlistId);

            if (playlist == null)
                return HttpNotFound();

            if (selectedSongIds != null)
            {
                foreach (int songId in selectedSongIds.Distinct())
                {
                    bool songExists = db.Songs.Any(s => s.SongId == songId);

                    bool alreadyAdded = db.PlaylistSongs.Any(
                        ps => ps.PlaylistId == playlistId &&
                              ps.SongId == songId);

                    if (songExists && !alreadyAdded)
                    {
                        db.PlaylistSongs.Add(new PlaylistSong
                        {
                            PlaylistId = playlistId,
                            SongId = songId
                        });
                    }
                }

                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = playlistId });
        }

        // POST: Playlist/RemoveSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveSong(int playlistId, int songId)
        {
            var playlist = db.Playlists.Find(playlistId);

            if (playlist == null)
                return HttpNotFound();

            var playlistSong = db.PlaylistSongs.FirstOrDefault(
                ps => ps.PlaylistId == playlistId &&
                      ps.SongId == songId);

            if (playlistSong != null)
            {
                db.PlaylistSongs.Remove(playlistSong);
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = playlistId });
        }

        // GET: Playlist/Delete/5
        public ActionResult Delete(int id)
        {
            var playlist = db.Playlists.Find(id);

            if (playlist == null)
                return HttpNotFound();

            return View(playlist);
        }

        // POST: Playlist/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var playlist = db.Playlists.Find(id);

            if (playlist == null)
                return HttpNotFound();

            // Delete the playlist's song relationships first
            var playlistSongs = db.PlaylistSongs
                .Where(ps => ps.PlaylistId == id)
                .ToList();

            db.PlaylistSongs.RemoveRange(playlistSongs);

            // Then delete the playlist itself
            db.Playlists.Remove(playlist);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // Dispose database connection
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
