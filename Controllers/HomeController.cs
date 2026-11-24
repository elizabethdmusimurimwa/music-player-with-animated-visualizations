using System.Data.Entity;   // needed for Include()
using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class HomeController : Controller
    {
        private readonly GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Home
        public ActionResult Index()
        {
            var songs = db.Songs
                          .Include(s => s.Artist)
                          .OrderBy(s => s.Title)
                          .ToList();

            ViewBag.Search = null;

            return View(songs);
        }

        // GET: Home/Search?query=abc
        public ActionResult Search(string query)
        {
            var songs = db.Songs
                          .Include(s => s.Artist)
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                songs = songs.Where(s =>
                    s.Title.Contains(query) ||
                    s.Album.Contains(query) ||
                    s.Artist.ArtistName.Contains(query)
                );
            }

            ViewBag.Search = query;

            return View("Index", songs.OrderBy(s => s.Title).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
