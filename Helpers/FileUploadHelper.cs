using System;
using System.IO;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Helpers
{
    // Small helper for saving uploaded audio files and images.
    public static class FileUploadHelper
    {
        public static readonly string[] AudioTypes = { ".mp3", ".wav", ".ogg", ".m4a" };
        public static readonly string[] ImageTypes = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public const int MaxAudioMb = 25;
        public const int MaxImageMb = 3;

        // True when the user actually picked a file
        public static bool HasFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0;
        }

        // Returns an error message, or "" when the file is fine
        public static string Check(HttpPostedFileBase file, string[] allowedTypes, int maxMb)
        {
            string extension = Path.GetExtension(file.FileName ?? "").ToLower();

            if (!allowedTypes.Contains(extension))
            {
                return "Allowed types: " + string.Join(", ", allowedTypes) + ".";
            }
            if (file.ContentLength > maxMb * 1024 * 1024)
            {
                return "File is too large (max " + maxMb + " MB).";
            }
            return "";
        }

        // Saves the file into the folder (for example "~/Content/Music/")
        // and returns the path to store in the database.
        public static string Save(HttpPostedFileBase file, string folder)
        {
            string folderOnDisk = HttpContext.Current.Server.MapPath(folder);
            Directory.CreateDirectory(folderOnDisk);

            // Keep only safe characters from the original name
            string baseName = Path.GetFileNameWithoutExtension(file.FileName);
            baseName = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            if (baseName == "")
            {
                baseName = "file";
            }

            // A short random prefix stops two uploads with the same name from overwriting each other
            string prefix = Guid.NewGuid().ToString("N").Substring(0, 8);
            string fileName = prefix + "-" + baseName + Path.GetExtension(file.FileName).ToLower();

            file.SaveAs(Path.Combine(folderOnDisk, fileName));
            return folder + fileName;
        }
    }
}
