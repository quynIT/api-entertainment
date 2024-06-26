using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models
{
    public class ReplyComment
    {
        [Key]
        public string? ReplyId { get; set; }
        public string? ReplyContent { get; set; }
        public DateOnly? ReplyPostingTime { get; set; }
        public int Like { get; set; }
        public int DisLike { get; set; }

        [ForeignKey("Comment")]
        public string? CommentId { get; set; }
        public virtual Comment? Comment { get; set; }

        [ForeignKey("AppUser")]
        public string? UserId { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}
