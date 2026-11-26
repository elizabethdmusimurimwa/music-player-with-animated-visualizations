using System.ComponentModel.DataAnnotations;

namespace GrooveyTunes.Models
{
    // One row in the Genres table.
    // Songs link to a genre through Song.GenreId 
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        [Required]
        [StringLength(40)]
        public string GenreName { get; set; }
    }
}
