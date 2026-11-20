using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GrooveyTunes.Models
{
    public class GrooveyTunesContext : DbContext
    {
        public GrooveyTunesContext()
            : base("name=GroovyTunesDB")
        {
        }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Song> Songs { get; set; }

        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }
    }
}