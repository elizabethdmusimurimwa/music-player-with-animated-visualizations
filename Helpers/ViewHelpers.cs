using System.Web.Mvc;

namespace GrooveyTunes.Helpers
{
    public static class ViewHelpers
    {
        // Returns "invalid" when the server found an error for this field,
        // so the red neon style shows even before the JavaScript runs.
        public static string FieldClass(this HtmlHelper html, string key)
        {
            ModelState state;
            if (html.ViewData.ModelState.TryGetValue(key, out state) && state.Errors.Count > 0)
            {
                return "invalid";
            }
            return "";
        }
    }
}
