using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Models
{
    public class PlaylistSong
    {
        [Key]
        public int PlaylistSongId { get; set; }

        public int PlaylistId { get; set; }

        public int SongId { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }
    }
}