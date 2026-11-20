using GrooveyTunes.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace GrooveyTunes.Controllers
{
    public class AccountController : Controller
    {
        private GrooveyTunesContext db = new GrooveyTunesContext();

        // ---------- LOGIN ----------

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // 1. Did the form pass the rules in LoginViewModel?
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Does this user exist, and is the password right?
            string email = model.Email.Trim().ToLower();
            AppUser user = db.AppUsers.FirstOrDefault(u => u.Email == email);

            if (user == null || PasswordHelper.Hash(model.Password, user.Salt) != user.PasswordHash)
            {
                // Same message for both cases so nobody can guess which emails are registered
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }

            // 3. Log them in
            SignIn(user, model.RememberMe);

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // ---------- SIGN UP ----------

        [HttpGet]
        public ActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(SignupViewModel model)
        {
            // Checks that attributes can't express on their own
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", "You need to accept the terms to continue.");
            }

            string username = (model.Username ?? "").Trim();
            string email = (model.Email ?? "").Trim().ToLower();

            if (username != "" && db.AppUsers.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "That username is taken.");
            }
            if (email != "" && db.AppUsers.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The very first account created becomes the Admin. Everyone after that is a normal User.
            string role = db.AppUsers.Any() ? "User" : "Admin";

            string salt = PasswordHelper.CreateSalt();
            AppUser user = new AppUser
            {
                Username = username,
                Email = email,
                Salt = salt,
                PasswordHash = PasswordHelper.Hash(model.Password, salt),
                Role = role,
                CreatedAt = DateTime.Now
            };
            db.AppUsers.Add(user);
            db.SaveChanges();

            // TempData survives one redirect, so the Login page can show this message
            TempData["Message"] = "Account created. Log in to start listening.";
            return RedirectToAction("Login");
        }

        // ---------- LOGOUT ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // ---------- helpers ----------

        // Creates the login cookie. The user's role is stored inside it (see Global.asax).
        private void SignIn(AppUser user, bool remember)
        {
            DateTime expires = remember ? DateTime.Now.AddDays(14) : DateTime.Now.AddHours(2);

            var ticket = new FormsAuthenticationTicket(
                1, user.Username, DateTime.Now, expires, remember, user.Role);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            if (remember)
            {
                cookie.Expires = expires;
            }
            Response.Cookies.Add(cookie);
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
