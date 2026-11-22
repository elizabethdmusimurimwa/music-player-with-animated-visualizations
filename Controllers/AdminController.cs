using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    // Admin dashboard: totals and the most recently added songs
    public class AdminController : AdminBaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                SongCount = db.Songs.Count(),
                ArtistCount = db.Artists.Count(),
                GenreCount = db.Genres.Count(),
                UserCount = db.AppUsers.Count(),
                PlaylistCount = db.Playlists.Count(),

                // Newest songs have the biggest SongId
                Recent = ToSongRows(db.Songs).OrderByDescending(r => r.SongId).Take(5).ToList(),

                TopGenres = db.Genres
                              .Select(g => new GenreRow
                              {
                                  GenreId = g.GenreId,
                                  GenreName = g.GenreName,
                                  SongCount = db.Songs.Count(s => s.GenreId == g.GenreId)
                              })
                              .OrderByDescending(g => g.SongCount)
                              .ThenBy(g => g.GenreName)
                              .Take(5)
                              .ToList()
            };

            return View(model);
        }
    }
}
