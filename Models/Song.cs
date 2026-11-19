using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; }

        [Required]
        public string Title { get; set; }

        public int ArtistId { get; set; }

        public int GenreId { get; set; }

        public string Album { get; set; }

        public string AudioPath { get; set; }

        public string ImagePath { get; set; }

        public int Duration { get; set; }

        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }
    }
}