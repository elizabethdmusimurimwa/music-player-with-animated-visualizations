using System.Linq;
using System.Web.Mvc;
using GrooveyTunes.Models;

namespace GrooveyTunes.Controllers
{
    public class AdminUsersController : AdminBaseController
    {
        // GET: AdminUsers?search=sam&page=1
        public ActionResult Index(string search, int page = 1)
        {
            var users = db.AppUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                users = users.Where(u => u.Username.Contains(term) || u.Email.Contains(term));
            }

            int total = users.Count();
            var pager = PagerInfo.Create(page, total, PageSize);

            var model = new UserListViewModel
            {
                Users = users.OrderBy(u => u.Username)
                             .Skip((pager.Page - 1) * PageSize)
                             .Take(PageSize)
                             .Select(u => new UserRow
                             {
                                 AppUserId = u.AppUserId,
                                 Username = u.Username,
                                 Email = u.Email,
                                 Role = u.Role,
                                 CreatedAt = u.CreatedAt,
                                 PlaylistCount = db.Playlists.Count(p => p.UserId == u.AppUserId)
                             })
                             .ToList(),
                Pager = pager,
                Search = search,
                MyId = CurrentUserId(),
                AdminCount = db.AppUsers.Count(u => u.Role == "Admin")
            };

            return View(model);
        }

        // ---------- PROMOTE ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Promote(int id)
        {
            AppUser user = db.AppUsers.Find(id);
            if (user == null)
            {
                Fail("That user no longer exists.");
                return RedirectToAction("Index");
            }

            if (user.Role == "Admin")
            {
                Fail(user.Username + " is already an admin.");
                return RedirectToAction("Index");
            }

            user.Role = "Admin";
            db.SaveChanges();

            Success(user.Username + " is now an admin.");
            return RedirectToAction("Index");
        }

        // ---------- DEMOTE ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Demote(int id)
        {
            AppUser user = db.AppUsers.Find(id);
            if (user == null)
            {
                Fail("That user no longer exists.");
                return RedirectToAction("Index");
            }

            // Safety rules
            if (user.AppUserId == CurrentUserId())
            {
                Fail("You can't remove your own admin access.");
                return RedirectToAction("Index");
            }
            if (user.Role == "Admin" && db.AppUsers.Count(u => u.Role == "Admin") <= 1)
            {
                Fail("There must always be at least one admin.");
                return RedirectToAction("Index");
            }

            user.Role = "User";
            db.SaveChanges();

            Success(user.Username + " is now a regular user.");
            return RedirectToAction("Index");
        }

        // ---------- DELETE ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            AppUser user = db.AppUsers.Find(id);
            if (user == null)
            {
                Fail("That user was already removed.");
                return RedirectToAction("Index");
            }

            // Safety rules
            if (user.AppUserId == CurrentUserId())
            {
                Fail("You can't delete your own account.");
                return RedirectToAction("Index");
            }
            if (user.Role == "Admin" && db.AppUsers.Count(u => u.Role == "Admin") <= 1)
            {
                Fail("The last admin can't be deleted.");
                return RedirectToAction("Index");
            }

            // Delete the user's playlists first (and the songs listed inside them)
            var playlistIds = db.Playlists.Where(p => p.UserId == id).Select(p => p.PlaylistId).ToList();
            var playlistSongs = db.PlaylistSongs.Where(ps => playlistIds.Contains(ps.PlaylistId)).ToList();
            db.PlaylistSongs.RemoveRange(playlistSongs);

            var playlists = db.Playlists.Where(p => p.UserId == id).ToList();
            db.Playlists.RemoveRange(playlists);

            db.AppUsers.Remove(user);
            db.SaveChanges();

            Success(user.Username + " and their playlists were deleted.");
            return RedirectToAction("Index");
        }
    }
}
