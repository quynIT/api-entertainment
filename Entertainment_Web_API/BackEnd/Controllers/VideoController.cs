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
    public class VideoController : ControllerBase
    {
        private readonly EntertainmentContext _context;
        private readonly string apiKey = "AIzaSyAHK-ZURhPgkkphHFT1szmPr6Dhx_zYH1M"; // Api key

        public VideoController(EntertainmentContext context)
        {
            _context = context;
        }

        [HttpGet("{searchTerm}")]
        public async Task<IActionResult> Get(string searchTerm)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = searchTerm;
            searchRequest.MaxResults = 32;
            searchRequest.Type = "video";
            searchRequest.VideoCategoryId = "10";
            searchRequest.EventType = SearchResource.ListRequest.EventTypeEnum.None;

            var searchResponse = await searchRequest.ExecuteAsync();

            var videos = new List<Video>();

            foreach (var item in searchResponse.Items)
            {
                var videoRequest = youtubeService.Videos.List("snippet,statistics");
                videoRequest.Id = item.Id.VideoId;

                var videoResponse = await videoRequest.ExecuteAsync();
                var video = videoResponse.Items[0];

                videos.Add(new Video
                {
                    VideoId = video.Id,
                    Title = video.Snippet.Title,
                    ThumbnailUrl = video.Snippet.Thumbnails.High.Url,
                    VideoUrl = $"https://www.youtube.com/watch?v={video.Id}",
                    VideoViews = (int?)video.Statistics.ViewCount.GetValueOrDefault(),
                    VideoPostingTime = video.Snippet.PublishedAtDateTimeOffset
                });
            }

            return Ok(videos);
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetDetails(string videoId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var videoRequest = youtubeService.Videos.List("snippet,statistics");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();

            // Tìm video trong cơ sở dữ liệu
            var existingVideo = await _context.Videos.FindAsync(videoId);

            var video = videoResponse.Items.Select(item => new Video
            {
                VideoId = item.Id,
                Title = item.Snippet.Title,
                ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                VideoUrl = $"https://www.youtube.com/embed/{item.Id}", // Chỉnh embed để phát được video
                VideoViews = (int?)item.Statistics.ViewCount.GetValueOrDefault(),
                Likes = existingVideo != null ? existingVideo.Likes : 0,
                Dislikes = existingVideo != null ? existingVideo.Dislikes : 0
            }).FirstOrDefault();

            return Ok(video);
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetRelatedVideos(string videoId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });

            // Lấy chi tiết video hiện tại
            var videoRequest = youtubeService.Videos.List("snippet");
            videoRequest.Id = videoId;
            var videoResponse = await videoRequest.ExecuteAsync();
            var currentVideo = videoResponse.Items[0];

            // Tìm kiếm các vid liên quan
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = currentVideo.Snippet.Title; // Cho tiêu đề vid hiện tại để tìm kiếm
            searchRequest.MaxResults = 5;
            searchRequest.Type = "video"; // Chỉ tìm các vid

            var searchResponse = await searchRequest.ExecuteAsync();

            var relatedVideos = new List<Video>();

            foreach (var searchResult in searchResponse.Items)
            {
                // Bỏ qua video nếu trùng
                if (searchResult.Id.VideoId != videoId)
                {
                    relatedVideos.Add(new Video
                    {
                        VideoId = searchResult.Id.VideoId,
                        Title = searchResult.Snippet.Title,
                        ThumbnailUrl = searchResult.Snippet.Thumbnails.Default__.Url,
                    });
                }
            }

            return Ok(relatedVideos);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserCommentedVideos(string userId)
        {
            // Lấy danh sách các Comment mà người dùng đã tạo
            var userComments = await _context.Comments
                .Where(c => c.Id == userId)
                .Select(c => c.VideoId)
                .Distinct()
                .ToListAsync();

            // Lấy danh sách các ReplyComment mà người dùng đã tạo
            var userReplyComments = await _context.ReplyComments
                .Where(rc => rc.UserId == userId)
                .Select(rc => rc.Comment.VideoId)
                .Distinct()
                .ToListAsync();

            // Gộp hai danh sách VideoId và loại bỏ các giá trị trùng lặp
            var commentedVideoIds = userComments.Union(userReplyComments).ToList();

            // Lấy danh sách các video từ các VideoId
            var commentedVideos = await _context.Videos
                .Where(video => commentedVideoIds.Contains(video.VideoId))
                .ToListAsync();

            return Ok(commentedVideos);
        }
    }
}