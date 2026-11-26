using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Helpers;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class AdminArtistsController : AdminBaseController
    {
        // GET: AdminArtists?search=ari&page=1
        public ActionResult Index(string search, int page = 1)
        {
            var artists = db.Artists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                artists = artists.Where(a => a.ArtistName.Contains(term));
            }

            int total = artists.Count();
            var pager = PagerInfo.Create(page, total, PageSize);

            var model = new ArtistListViewModel
            {
                Artists = artists.OrderBy(a => a.ArtistName)
                                 .Skip((pager.Page - 1) * PageSize)
                                 .Take(PageSize)
                                 .Select(a => new ArtistRow
                                 {
                                     ArtistId = a.ArtistId,
                                     ArtistName = a.ArtistName,
                                     ImagePath = a.ImagePath,
                                     SongCount = db.Songs.Count(s => s.ArtistId == a.ArtistId)
                                 })
                                 .ToList(),
                Pager = pager,
                Search = search
            };

            return View(model);
        }

        // ADD 

        public ActionResult Create()
        {
            ViewBag.Heading = "Add an artist";
            return View("Form", new ArtistFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ArtistFormViewModel model)
        {
            CheckArtist(model);
            if (!ModelState.IsValid)
            {
                ViewBag.Heading = "Add an artist";
                return View("Form", model);
            }

            Artist artist = new Artist();
            CopyToArtist(model, artist);
            db.Artists.Add(artist);
            db.SaveChanges();

            Success(artist.ArtistName + " was added.");
            return RedirectToAction("Index");
        }

        // EDIT 

        public ActionResult Edit(int id)
        {
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                Fail("That artist no longer exists.");
                return RedirectToAction("Index");
            }

            var model = new ArtistFormViewModel
            {
                ArtistId = artist.ArtistId,
                ArtistName = artist.ArtistName,
                Biography = artist.Biography,
                ImagePath = artist.ImagePath
            };
            ViewBag.Heading = "Edit artist";
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ArtistFormViewModel model)
        {
            Artist artist = db.Artists.Find(model.ArtistId);
            if (artist == null)
            {
                Fail("That artist no longer exists.");
                return RedirectToAction("Index");
            }

            CheckArtist(model);
            if (!ModelState.IsValid)
            {
                ViewBag.Heading = "Edit artist";
                return View("Form", model);
            }

            CopyToArtist(model, artist);
            db.SaveChanges();

            Success(artist.ArtistName + " was updated.");
            return RedirectToAction("Index");
        }

        // DELETE 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                Fail("That artist was already removed.");
                return RedirectToAction("Index");
            }

            // Songs can't exist without an artist, so block the delete while any remain
            int songCount = db.Songs.Count(s => s.ArtistId == id);
            if (songCount > 0)
            {
                Fail(artist.ArtistName + " still has " + songCount + (songCount == 1 ? " song" : " songs") +
                     ". Delete those songs first.");
                return RedirectToAction("Index");
            }

            db.Artists.Remove(artist);
            db.SaveChanges();

            Success(artist.ArtistName + " was deleted.");
            return RedirectToAction("Index");
        }

        // helpers

        private void CheckArtist(ArtistFormViewModel model)
        {
            string name = (model.ArtistName ?? "").Trim();

            if (name != "" && db.Artists.Any(a => a.ArtistName == name && a.ArtistId != model.ArtistId))
            {
                ModelState.AddModelError("ArtistName", "An artist with this name already exists.");
            }

            if (FileUploadHelper.HasFile(model.ImageFile))
            {
                string problem = FileUploadHelper.Check(model.ImageFile, FileUploadHelper.ImageTypes, FileUploadHelper.MaxImageMb);
                if (problem != "")
                {
                    ModelState.AddModelError("ImageFile", problem);
                }
            }
        }

        private void CopyToArtist(ArtistFormViewModel model, Artist artist)
        {
            artist.ArtistName = model.ArtistName.Trim();
            artist.Biography = string.IsNullOrWhiteSpace(model.Biography) ? null : model.Biography.Trim();

            if (FileUploadHelper.HasFile(model.ImageFile))
            {
                artist.ImagePath = FileUploadHelper.Save(model.ImageFile, "~/Content/Images/");
            }
            else
            {
                artist.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();
            }
        }
    }
}
