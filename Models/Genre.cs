using System.ComponentModel.DataAnnotations;

namespace GrooveyTunes.Models
{
    // One row in the Genres table.
    // Songs link to a genre through Song.GenreId (the Song class is left exactly as it was).
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        [Required]
        [StringLength(40)]
        public string GenreName { get; set; }
    }
}
