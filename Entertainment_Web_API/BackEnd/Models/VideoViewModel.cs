namespace BackEnd.Models
{
    public class VideoViewModel
    {
        public Video? Video { get; set; }
        public List<ReplyComment>? ReplyComments { get; set; }
        public List<Playlist>? Playlists { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
