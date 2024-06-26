using BackEnd.Data;
using BackEnd.Models;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [Route("backend/[controller]/[action]")]
    [ApiController]
    public class LikeDislikeController : ControllerBase
    {
        private readonly EntertainmentContext _context;
        private readonly string apiKey = "AIzaSyAHK-ZURhPgkkphHFT1szmPr6Dhx_zYH1M"; // Api key

        public LikeDislikeController(EntertainmentContext context)
        {
            _context = context;
        }

        [HttpGet("{videoId}/{userId}")]
        public async Task<IActionResult> LikeVideo(string videoId, string userId)
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
                    VideoPostingTime = video.Snippet.PublishedAtDateTimeOffset,
                    Likes = 0,
                    Dislikes = 0,
                };

                // Thêm Video vào cơ sở dữ liệu
                _context.Videos.Add(newVideo);
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }

            var findVideo = await _context.Videos.FindAsync(videoId);
            if (findVideo == null) // Kiểm tra findVideo
            {
                return NotFound();
            }

            findVideo.Likes += 1;

            // Kiểm tra xem người dùng đã like video này trước đó hay chưa
            var userVideoReaction = await _context.UserVideoReactions
                .FirstOrDefaultAsync(uvr => uvr.Id == userId && uvr.VideoId == videoId);

            if (userVideoReaction == null)
            {
                // Nếu người dùng chưa like video này trước đó, tạo một UserVideoReaction mới
                userVideoReaction = new UserVideoReaction
                {
                    Id = userId,
                    VideoId = videoId,
                    IsLiked = true,
                    IsDisliked = false
                };
                _context.UserVideoReactions.Add(userVideoReaction);
            }
            else
            {
                // Nếu người dùng đã like video này trước đó, cập nhật trạng thái
                userVideoReaction.IsLiked = true;
                userVideoReaction.IsDisliked = false;
            }

            await _context.SaveChangesAsync();

            return Ok(findVideo.Likes);
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> UndoLikeVideo(string videoId)
        {
            var findVideo = await _context.Videos.FindAsync(videoId);
            if (findVideo == null) // Kiểm tra findVideo
            {
                return NotFound();
            }

            findVideo.Likes -= 1;
            await _context.SaveChangesAsync();

            return Ok(findVideo.Likes);
        }

        [HttpGet("{videoId}/{userId}")]
        public async Task<IActionResult> DislikeVideo(string videoId, string userId)
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

            var findVideo = await _context.Videos.FindAsync(videoId);
            if (video == null)
            {
                return NotFound();
            }

            findVideo.Dislikes += 1;

            // Kiểm tra xem người dùng đã dislike video này trước đó hay chưa
            var userVideoReaction = await _context.UserVideoReactions
                .FirstOrDefaultAsync(uvr => uvr.Id == userId && uvr.VideoId == videoId);

            if (userVideoReaction == null)
            {
                // Nếu người dùng chưa dislike video này trước đó, tạo một UserVideoReaction mới
                userVideoReaction = new UserVideoReaction
                {
                    Id = userId,
                    VideoId = videoId,
                    IsLiked = false,
                    IsDisliked = true
                };
                _context.UserVideoReactions.Add(userVideoReaction);
            }
            else
            {
                // Nếu người dùng đã dislike video này trước đó, cập nhật trạng thái
                userVideoReaction.IsLiked = false;
                userVideoReaction.IsDisliked = true;
            }

            await _context.SaveChangesAsync();

            return Ok(findVideo.Dislikes);
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> UndoDislikeVideo(string videoId)
        {
            var findVideo = await _context.Videos.FindAsync(videoId);
            if (findVideo == null) // Kiểm tra findVideo
            {
                return NotFound();
            }

            findVideo.Dislikes -= 1;
            await _context.SaveChangesAsync();

            return Ok(findVideo.Dislikes);
        }

        [HttpGet("{videoId}/{userId}")]
        public async Task<IActionResult> CheckLikeStatus(string videoId, string userId)
        {
            // Kiểm tra xem người dùng đã like video này trước đó hay chưa
            var userVideoReaction = await _context.UserVideoReactions
                .FirstOrDefaultAsync(uvr => uvr.Id == userId && uvr.VideoId == videoId);

            if (userVideoReaction != null && userVideoReaction.IsLiked)
            {
                // Nếu người dùng đã like video này trước đó, trả về true
                return Ok(true);
            }

            // Nếu người dùng chưa like video này trước đó, trả về false
            return Ok(false);
        }

        [HttpGet("{videoId}/{userId}")]
        public async Task<IActionResult> CheckDislikeStatus(string videoId, string userId)
        {
            // Kiểm tra xem người dùng đã dislike video này trước đó hay chưa
            var userVideoReaction = await _context.UserVideoReactions
                .FirstOrDefaultAsync(uvr => uvr.Id == userId && uvr.VideoId == videoId);

            if (userVideoReaction != null && userVideoReaction.IsDisliked)
            {
                // Nếu người dùng đã dislike video này trước đó, trả về true
                return Ok(true);
            }

            // Nếu người dùng chưa dislike video này trước đó, trả về false
            return Ok(false);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetLikedVideos(string userId)
        {
            // Lấy danh sách các UserVideoReaction mà người dùng đã "like"
            var likedUserVideoReactions = await _context.UserVideoReactions
                .Where(uvr => uvr.Id == userId && uvr.IsLiked)
                .ToListAsync();

            // Lấy danh sách các VideoId từ các UserVideoReaction
            var likedVideoIds = likedUserVideoReactions.Select(uvr => uvr.VideoId).ToList();

            // Lấy danh sách các video từ các VideoId
            var likedVideos = await _context.Videos
                .Where(video => likedVideoIds.Contains(video.VideoId))
                .ToListAsync();

            return Ok(likedVideos);
        }
    }
}
