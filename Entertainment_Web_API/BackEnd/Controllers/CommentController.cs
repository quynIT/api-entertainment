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
    public class CommentController : ControllerBase
    {
        private readonly EntertainmentContext _context;
        private readonly string apiKey = "AIzaSyAHK-ZURhPgkkphHFT1szmPr6Dhx_zYH1M"; // Api key

        public CommentController(EntertainmentContext context)
        {
            _context = context;
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetComments(string videoId)
        {
            var comment = await _context.Comments
                .Include(v => v.Video)
                .Include(u => u.AppUser)
                .Where(c => c.VideoId == videoId).ToListAsync();

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpPost("{userId}/{videoId}/{content}")]
        public async Task<IActionResult> AddComment(string userId, string videoId, string content)
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
                    Likes = existingVideo != null ? existingVideo.Likes : 0,
                    Dislikes = existingVideo != null ? existingVideo.Dislikes : 0
                };

                // Thêm Video vào cơ sở dữ liệu
                _context.Videos.Add(newVideo);
            }

            // Tạo mới comment
            var newComment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                Content = content,
                CommentPostingTime = DateOnly.FromDateTime(DateTime.Now),
                Id = userId,
                VideoId = videoId
            };

            // Thêm comment vào cơ sở dữ liệu
            _context.Comments.Add(newComment);

            await _context.SaveChangesAsync();

            return Ok(newComment);
        }

        [HttpPut("{commentId}/{content}")]
        public async Task<IActionResult> UpdateComment(string commentId, string content)
        {
            if (ModelState.IsValid)
            {
                var existingComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (existingComment != null)
                {
                    existingComment.Content = content;

                    await _context.SaveChangesAsync();

                    return Ok();
                }
            }

            return BadRequest();
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return BadRequest();

            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
