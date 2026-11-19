using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [Required]
        public string PlaylistName { get; set; }

        public int UserId { get; set; }

        public string ImagePath { get; set; }

        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; }
    }
}