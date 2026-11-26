using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class AdminGenresController : AdminBaseController
    {
        // GET: AdminGenres?search=po&page=1
        public ActionResult Index(string search, int page = 1)
        {
            var genres = db.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                genres = genres.Where(g => g.GenreName.Contains(term));
            }

            int total = genres.Count();
            var pager = PagerInfo.Create(page, total, PageSize);

            var model = new GenreListViewModel
            {
                Genres = genres.OrderBy(g => g.GenreName)
                               .Skip((pager.Page - 1) * PageSize)
                               .Take(PageSize)
                               .Select(g => new GenreRow
                               {
                                   GenreId = g.GenreId,
                                   GenreName = g.GenreName,
                                   SongCount = db.Songs.Count(s => s.GenreId == g.GenreId)
                               })
                               .ToList(),
                Pager = pager,
                Search = search
            };

            return View(model);
        }

        //ADD

        public ActionResult Create()
        {
            ViewBag.Heading = "Add a genre";
            return View("Form", new GenreFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GenreFormViewModel model)
        {
            CheckGenre(model);
            if (!ModelState.IsValid)
            {
                ViewBag.Heading = "Add a genre";
                return View("Form", model);
            }

            Genre genre = new Genre { GenreName = model.GenreName.Trim() };
            db.Genres.Add(genre);
            db.SaveChanges();

            Success(genre.GenreName + " was added.");
            return RedirectToAction("Index");
        }

        //EDIT

        public ActionResult Edit(int id)
        {
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                Fail("That genre no longer exists.");
                return RedirectToAction("Index");
            }

            ViewBag.Heading = "Edit genre";
            return View("Form", new GenreFormViewModel { GenreId = genre.GenreId, GenreName = genre.GenreName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GenreFormViewModel model)
        {
            Genre genre = db.Genres.Find(model.GenreId);
            if (genre == null)
            {
                Fail("That genre no longer exists.");
                return RedirectToAction("Index");
            }

            CheckGenre(model);
            if (!ModelState.IsValid)
            {
                ViewBag.Heading = "Edit genre";
                return View("Form", model);
            }

            genre.GenreName = model.GenreName.Trim();
            db.SaveChanges();

            Success(genre.GenreName + " was updated.");
            return RedirectToAction("Index");
        }

        //DELETE 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Genre genre = db.Genres.Find(id);
            if (genre == null)
            {
                Fail("That genre was already removed.");
                return RedirectToAction("Index");
            }

            // A genre that still has songs can't be deleted
            int songCount = db.Songs.Count(s => s.GenreId == id);
            if (songCount > 0)
            {
                Fail(genre.GenreName + " still has " + songCount + (songCount == 1 ? " song" : " songs") +
                     ". Move or delete those songs first.");
                return RedirectToAction("Index");
            }

            db.Genres.Remove(genre);
            db.SaveChanges();

            Success(genre.GenreName + " was deleted.");
            return RedirectToAction("Index");
        }

        // helpers

        private void CheckGenre(GenreFormViewModel model)
        {
            string name = (model.GenreName ?? "").Trim();

            if (name != "" && db.Genres.Any(g => g.GenreName == name && g.GenreId != model.GenreId))
            {
                ModelState.AddModelError("GenreName", "That genre already exists.");
            }
        }
    }
}
