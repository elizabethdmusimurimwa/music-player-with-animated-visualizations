using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace GrooveyTunes.Models
{
    // ============================================================
    //  Paging (tables show 10 rows per page)
    // ============================================================
    public class PagerInfo
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }

        public int FirstItem { get { return TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1; } }
        public int LastItem { get { return Math.Min(Page * PageSize, TotalItems); } }

        // Makes sure the page number is always inside 1..TotalPages
        public static PagerInfo Create(int page, int totalItems, int pageSize)
        {
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            return new PagerInfo
            {
                Page = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
        }
    }

    // ============================================================
    //  Rows shown in the admin tables
    // ============================================================
    public class SongRow
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string GenreName { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public string ImagePath { get; set; }
        public int PlaylistCount { get; set; }
    }

    public class ArtistRow
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public string ImagePath { get; set; }
        public int SongCount { get; set; }
    }

    public class GenreRow
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int SongCount { get; set; }
    }

    public class UserRow
    {
        public int AppUserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlaylistCount { get; set; }
    }

    // ============================================================
    //  Whole-page models for the list pages
    // ============================================================
    public class DashboardViewModel
    {
        public int SongCount { get; set; }
        public int ArtistCount { get; set; }
        public int GenreCount { get; set; }
        public int UserCount { get; set; }
        public int PlaylistCount { get; set; }
        public List<SongRow> Recent { get; set; }
        public List<GenreRow> TopGenres { get; set; }
    }

    public class SongListViewModel
    {
        public List<SongRow> Songs { get; set; }
        public PagerInfo Pager { get; set; }
        public string Search { get; set; }
        public int? ArtistId { get; set; }
        public int? GenreId { get; set; }
        public SelectList Artists { get; set; }
        public SelectList Genres { get; set; }
    }

    public class ArtistListViewModel
    {
        public List<ArtistRow> Artists { get; set; }
        public PagerInfo Pager { get; set; }
        public string Search { get; set; }
    }

    public class GenreListViewModel
    {
        public List<GenreRow> Genres { get; set; }
        public PagerInfo Pager { get; set; }
        public string Search { get; set; }
    }

    public class UserListViewModel
    {
        public List<UserRow> Users { get; set; }
        public PagerInfo Pager { get; set; }
        public string Search { get; set; }
        public int MyId { get; set; }          // the admin who is looking at the page
        public int AdminCount { get; set; }    // how many admins exist right now
    }

    // ============================================================
    //  Form models (rules are repeated in Scripts/admin.js for instant feedback)
    // ============================================================
    public class SongFormViewModel
    {
        public int SongId { get; set; }

        [Required(ErrorMessage = "Enter the song title.")]
        [StringLength(100, ErrorMessage = "Title can be 100 characters at most.")]
        public string Title { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose an artist.")]
        public int ArtistId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose a genre.")]
        public int GenreId { get; set; }

        [StringLength(100, ErrorMessage = "Album can be 100 characters at most.")]
        public string Album { get; set; }

        [StringLength(500, ErrorMessage = "Path is too long (500 characters at most).")]
        [RegularExpression(@"^(~/|https?://).+", ErrorMessage = "Start the path with ~/ or the link with http:// or https://")]
        public string AudioPath { get; set; }

        [StringLength(500, ErrorMessage = "Path is too long (500 characters at most).")]
        [RegularExpression(@"^(~/|https?://).+", ErrorMessage = "Start the path with ~/ or the link with http:// or https://")]
        public string ImagePath { get; set; }

        [Range(0, 7200, ErrorMessage = "Enter whole seconds between 0 and 7200.")]
        public int? Duration { get; set; }

        // Optional uploads. If a file is chosen it replaces the path above.
        public HttpPostedFileBase AudioFile { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }

        // Drop-down contents (filled by the controller)
        public IEnumerable<SelectListItem> Artists { get; set; }
        public IEnumerable<SelectListItem> Genres { get; set; }
    }

    public class ArtistFormViewModel
    {
        public int ArtistId { get; set; }

        [Required(ErrorMessage = "Enter the artist's name.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2 to 100 characters.")]
        public string ArtistName { get; set; }

        [StringLength(2000, ErrorMessage = "Biography can be 2000 characters at most.")]
        public string Biography { get; set; }

        [StringLength(500, ErrorMessage = "Path is too long (500 characters at most).")]
        [RegularExpression(@"^(~/|https?://).+", ErrorMessage = "Start the path with ~/ or the link with http:// or https://")]
        public string ImagePath { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }

    public class GenreFormViewModel
    {
        public int GenreId { get; set; }

        [Required(ErrorMessage = "Enter the genre name.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Genre name must be 2 to 40 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 &'\-/]*$", ErrorMessage = "Use letters, numbers, spaces and & ' - / only.")]
        public string GenreName { get; set; }
    }

    // ============================================================
    //  Public Genres page (inside the friend's site layout)
    // ============================================================
    public class GenreDetailsViewModel
    {
        public Genre Genre { get; set; }
        public List<Song> Songs { get; set; }
    }
}
