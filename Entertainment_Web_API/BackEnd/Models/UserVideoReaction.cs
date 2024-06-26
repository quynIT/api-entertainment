using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Models
{
    public class UserVideoReaction
    {
        public bool IsLiked { get; set; }
        public bool IsDisliked { get; set; }

        [Key, Column(Order = 0)]
        [ForeignKey("Video")]
        public string? VideoId { get; set; }
        public virtual Video? Video { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("AppUser")]
        public string? Id { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}
