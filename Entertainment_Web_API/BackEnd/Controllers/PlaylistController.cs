using BackEnd.Data;
using BackEnd.Models;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [Route("backend/[controller]/[action]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly EntertainmentContext _context;
        private readonly string apiKey = "AIzaSyAHK-ZURhPgkkphHFT1szmPr6Dhx_zYH1M"; // Api key

        public PlaylistController(EntertainmentContext context)
        {
            _context = context;
        }

        // GET: api/Playlists
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetPlaylists(string userId)
        {          
            var playlists = await _context.Playlist.Where(p => p.Id == userId).ToListAsync();

            foreach (var playlist in playlists)
            {
                playlist.VideoCount = await _context.VideoPlaylists.CountAsync(vp => vp.PlaylistId == playlist.PlaylistId);
            }

            if (playlists == null || playlists.Count == 0)
            {
                return NotFound();
            }

            return Ok(playlists);
        }

        [HttpGet("{userId}/{playlistId}")]
        public async Task<IActionResult> GetPlaylistVideos(string userId, string playlistId)
        {
            // Tìm playlist dựa trên userId và playlistId
            var playlist = await _context.Playlist
                .Include(p => p.VideoPlaylists) // Include VideoPlaylists để lấy các video thuộc playlist
                .ThenInclude(vp => vp.Video) // Include Video để lấy thông tin video
                .FirstOrDefaultAsync(p => p.Id == userId && p.PlaylistId == playlistId);

            if (playlist == null)
            {
                return NotFound();
            }

            // Lấy danh sách video từ playlist
            var videos = playlist.VideoPlaylists.Select(vp => vp.Video).ToList();

            return Ok(videos);
        }

        [HttpPost("{playlistId}/{videoId}")]
        public async Task<IActionResult> AddVideoToPlaylist(string playlistId, string videoId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            // Lấy thông tin video từ YouTube API
            var videoRequest = youtubeService.Videos.List("snippet,statistics");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();
            var video = videoResponse.Items[0];

            // Kiểm tra xem video đã tồn tại trong cơ sở dữ liệu hay chưa
            var existingVideo = await _context.Videos.FindAsync(videoId);

            if (existingVideo == null)
            {
                // Tạo mới Video
                var newVideo = new Video
                {
                    VideoId = video.Id,
                    Title = video.Snippet.Title,
                    ThumbnailUrl = video.Snippet.Thumbnails.High.Url,
                    VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                    VideoViews = (int?)video.Statistics.ViewCount.GetValueOrDefault(),
                    VideoPostingTime = video.Snippet.PublishedAtDateTimeOffset
                };

                // Thêm Video vào cơ sở dữ liệu
                _context.Videos.Add(newVideo);
            }

            // Kiểm tra xem video đã tồn tại trong playlist chưa
            var existingVideoInPlaylist = await _context.VideoPlaylists.FirstOrDefaultAsync(vp => vp.PlaylistId == playlistId && vp.VideoId == videoId);

            if (existingVideoInPlaylist != null)
            {
                return BadRequest(new { success = false, message = "Video already exists in the playlist!" });
            }

            // Tạo mới VideoPlaylist
            var newVideoPlaylist = new VideoPlaylist
            {
                VideoId = videoId,
                PlaylistId = playlistId
            };

            // Thêm VideoPlaylist vào cơ sở dữ liệu
            _context.VideoPlaylists.Add(newVideoPlaylist);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Added video to playlist successfully!" });
        }

        [HttpPost("{userId}/{videoId}/{playlistName}")]
        public async Task<IActionResult> CreatePlaylist(string userId, string videoId, string playlistName)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            // Lấy thông tin video từ YouTube API
            var videoRequest = youtubeService.Videos.List("snippet,statistics");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();
            var video = videoResponse.Items[0];

            // Kiểm tra xem video đã tồn tại trong cơ sở dữ liệu hay chưa
            var existingVideo = await _context.Videos.FindAsync(videoId);

            if (existingVideo == null)
            {
                // Tạo mới Video
                var newVideo = new Video
                {
                    VideoId = video.Id,
                    Title = video.Snippet.Title,
                    ThumbnailUrl = video.Snippet.Thumbnails.High.Url,
                    VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                    VideoViews = (int?)video.Statistics.ViewCount.GetValueOrDefault(),
                    VideoPostingTime = video.Snippet.PublishedAtDateTimeOffset
                };

                // Thêm Video vào cơ sở dữ liệu
                _context.Videos.Add(newVideo);
            }

			// Kiểm tra xem tên playlist đã tồn tại hay chưa
			var existingPlaylist = await _context.Playlist.FirstOrDefaultAsync(p => p.PlaylistName == playlistName && p.Id == userId);

			if (existingPlaylist != null)
			{
				// Nếu tên playlist đã tồn tại, trả về thông báo lỗi
				return BadRequest("A playlist with this name already exists.");
			}

			// Tạo mới playlist
			var newPlaylist = new Playlist
            {
                PlaylistId = Guid.NewGuid().ToString(),
                PlaylistName = playlistName,
                ThumbnailUrl = existingVideo.ThumbnailUrl, // Lấy thumbnail cho biến ở trên gán cho thuộc tính của Playlist
                Id = userId
            };

            // Thêm playlist vào cơ sở dữ liệu
            _context.Playlist.Add(newPlaylist);

            // Tạo mới VideoPlaylist
            var newVideoPlaylist = new VideoPlaylist
            {
                VideoId = videoId,
                PlaylistId = newPlaylist.PlaylistId
            };

            // Thêm VideoPlaylist vào cơ sở dữ liệu
            _context.VideoPlaylists.Add(newVideoPlaylist);

            await _context.SaveChangesAsync();

            return Ok(newPlaylist);
        }

        [HttpPut("{playlistId}/{playlistName}")]
        public async Task<IActionResult> EditPlaylist(string playlistId, string playlistName)
        {
            if (ModelState.IsValid)
            {
                var existingPlaylist = await _context.Playlist.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

                if (existingPlaylist != null)
                {
                    existingPlaylist.PlaylistName = playlistName;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
            }

            return BadRequest();
        }

        [HttpDelete("{playlistId}")]
        public async Task<IActionResult> DeletePlaylist(string playlistId)
        {
            // Xóa các liên kết trong VideoPlaylist
            var videoPlaylists = _context.VideoPlaylists.Where(vp => vp.PlaylistId == playlistId);
            _context.VideoPlaylists.RemoveRange(videoPlaylists);

            // Xóa danh sách phát
            var playlist = await _context.Playlist.FindAsync(playlistId);
            if (playlist != null)
            {
                _context.Playlist.Remove(playlist);

                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete("{playlistId}/{videoId}")]
        public async Task<IActionResult> DeleteVideoFromPlaylist(string playlistId, string videoId)
        {
            // Tìm video trong playlist
            var videoPlaylist = await _context.VideoPlaylists.FirstOrDefaultAsync(vp => vp.PlaylistId == playlistId && vp.VideoId == videoId);

            if (videoPlaylist != null)
            {
                // Lấy danh sách video còn tồn tại trong playlist
                var existingVideos = await _context.Videos.Where(v => v.VideoPlaylists.Any(vp => vp.PlaylistId == playlistId)).ToListAsync();

                // Lấy thumbnail của video còn tồn tại
                var existingVideo = existingVideos.FirstOrDefault(v => v.VideoId != videoId);

                if (existingVideo != null)
                {
                    // Cập nhật thumbnail của playlist bằng thumbnail của video còn tồn tại
                    var playlistToUpdate = await _context.Playlist.FindAsync(playlistId);
                    if (playlistToUpdate != null)
                    {
                        playlistToUpdate.ThumbnailUrl = existingVideo.ThumbnailUrl;
                    }
                }

                // Xóa liên kết video-playlist
                _context.VideoPlaylists.Remove(videoPlaylist);

                await _context.SaveChangesAsync();

                return Ok(); // Xóa thành công
            }

            return BadRequest(); // Video không tồn tại trong playlist
        }
    }
}
