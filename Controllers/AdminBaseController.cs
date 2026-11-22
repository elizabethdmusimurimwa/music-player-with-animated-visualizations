using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Helpers;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    // Every admin controller inherits from this one, so the [AdminOnly] lock
    // and the shared helpers live in a single place.
    [AdminOnly]
    public abstract class AdminBaseController : Controller
    {
        protected GrooveyTunesContext db = new GrooveyTunesContext();

        protected const int PageSize = 10;   // rows per page in every table

        // Id of the admin who is logged in right now
        protected int CurrentUserId()
        {
            string name = User.Identity.Name;
            return db.AppUsers.Where(u => u.Username == name)
                              .Select(u => u.AppUserId)
                              .FirstOrDefault();
        }

        // Messages shown as a toast after the redirect (see _AdminLayout)
        protected void Success(string message)
        {
            TempData["Success"] = message;
        }

        protected void Fail(string message)
        {
            TempData["Error"] = message;
        }

        // Songs joined with their artist and genre name. A song whose genre
        // is missing (GenreId 0) still shows up, marked "Unassigned".
        protected IQueryable<SongRow> ToSongRows(IQueryable<Song> songs)
        {
            return from s in songs
                   join genre in db.Genres on s.GenreId equals genre.GenreId into genreGroup
                   from g in genreGroup.DefaultIfEmpty()
                   select new SongRow
                   {
                       SongId = s.SongId,
                       Title = s.Title,
                       ArtistName = s.Artist.ArtistName,
                       GenreName = g == null ? "Unassigned" : g.GenreName,
                       Album = s.Album,
                       Duration = s.Duration,
                       ImagePath = s.ImagePath,
                       PlaylistCount = db.PlaylistSongs.Count(p => p.SongId == s.SongId)
                   };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
