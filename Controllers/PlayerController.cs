using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class PlayerController : Controller
    {
        private GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Player
        public ActionResult Index()
        {
            var songs = db.Songs
                          .Include("Artist")
                          .ToList();

            return View(songs);
        }
    }
}
