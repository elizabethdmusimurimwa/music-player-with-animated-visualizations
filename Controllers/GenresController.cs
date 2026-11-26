using System.Data.Entity;   
using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    // The public Genres pages everyone can open from the side navigation
    public class GenresController : Controller
    {
        private readonly GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Genres
        public ActionResult Index()
        {
            var genres = db.Genres
                           .OrderBy(g => g.GenreName)
                           .Select(g => new GenreRow
                           {
                               GenreId = g.GenreId,
                               GenreName = g.GenreName,
                               SongCount = db.Songs.Count(s => s.GenreId == g.GenreId)
                           })
                           .ToList();

            return View(genres);
        }

        // GET: Genres/Details/3
        public ActionResult Details(int id)
        {
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                return HttpNotFound();
            }

            var model = new GenreDetailsViewModel
            {
                Genre = genre,
                Songs = db.Songs
                          .Include(s => s.Artist)
                          .Where(s => s.GenreId == id)
                          .OrderBy(s => s.Title)
                          .ToList()
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
