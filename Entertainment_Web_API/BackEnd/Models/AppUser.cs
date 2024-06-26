using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? UserNamee { get; set; }
        public string? Avtprofile { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public virtual ICollection<UserVideoReaction> UserVideoReactions { get; set; } = new List<UserVideoReaction>();
    }
}
