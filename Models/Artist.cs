using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Models
{
    public class Artist
    {
        [Key]
        public int ArtistId { get; set; }

        [Required]
        public string ArtistName { get; set; }

        public string ImagePath { get; set; }

        public string Biography { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
    }
}