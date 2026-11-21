using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class PlaylistController : Controller
    {
        private GrooveyTunesContext db = new GrooveyTunesContext();

        // GET: Playlist
        public ActionResult Index()
        {
            var playlists = db.Playlists.ToList();

            return View(playlists);
        }

        // GET: Playlist/Details/5
        public ActionResult Details(int id)
        {
            var playlist = db.Playlists
                .Include("PlaylistSongs.Song")
                .FirstOrDefault(p => p.PlaylistId == id);

            if (playlist == null)
            {
                return HttpNotFound();
            }

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
                // Temporary default until user accounts are implemented
                playlist.UserId = 1;

                db.Playlists.Add(playlist);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(playlist);
        }
    }
}
