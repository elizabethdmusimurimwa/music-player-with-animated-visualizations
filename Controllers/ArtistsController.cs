using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class ArtistsController : Controller
    {
        private GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Artists
        public ActionResult Index()
        {
            var artists = db.Artists.ToList();

            return View(artists);
        }

        // GET: Artists/Details/5
        public ActionResult Details(int id)
        {
            var artist = db.Artists
                           .Include("Songs")
                           .FirstOrDefault(a => a.ArtistId == id);

            if (artist == null)
            {
                return HttpNotFound();
            }

            return View(artist);
        }
    }
}
