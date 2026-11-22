using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GrooveyTunes.Models;

namespace GrooveyTunes.Helpers
{
    // Put [AdminOnly] on a controller to keep everyone except admins out.
    // The role is checked in the database on every request, so demoting or deleting
    // an admin takes effect straight away (not only when their login cookie expires).
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            string username = httpContext.User.Identity.Name;

            using (var db = new GrooveyTunesContext())
            {
                return db.AppUsers.Any(u => u.Username == username && u.Role == "Admin");
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Logged in, but not an admin: send them to the normal site
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Home", action = "Index" }));
            }
            else
            {
                // Not logged in: the normal behaviour sends them to the Login page
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
