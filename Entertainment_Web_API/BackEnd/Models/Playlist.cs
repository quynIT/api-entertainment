using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class Playlist
    {
        [Key]
        public string? PlaylistId { get; set; }
        public string? PlaylistName { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? VideoCount { get; set; }

		[ForeignKey("AppUser")]
		public string? Id { get; set; }
		public virtual AppUser? AppUser { get; set; }

		public virtual ICollection<VideoPlaylist> VideoPlaylists { get; set; } = new List<VideoPlaylist>();
    }
}
