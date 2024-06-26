using BackEnd.Models;
using Entertainment_Web_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace Entertainment_Web_API.Controllers
{
    public class HomeController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7142/backend");
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _httpContextAccessor = httpContextAccessor;
            _client.BaseAddress = baseAddress;

        }

        private string GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.User.Identity.IsAuthenticated)
            {
                return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // Nếu người dùng không đăng nhập, trả về null hoặc xử lý tùy theo yêu cầu
            return null;
        }

        public async Task<IActionResult> NoAccount(string searchTerm = "music")
        {
            List<Video> videoList = new List<Video>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Video/Get/{searchTerm}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                videoList = JsonConvert.DeserializeObject<List<Video>>(data);
            }
            return View(videoList);
        }

        //[Authorize]
        public async Task<IActionResult> Index(string searchTerm = "music", int pageNumber = 1)
        {
            List<Video> videoList = new List<Video>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Video/Get/{searchTerm}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var videos = JsonConvert.DeserializeObject<List<Video>>(data);

                int pageSize = 8; // Số lượng video trên mỗi trang
                ViewBag.SearchTerm = searchTerm; // Thêm dòng này để lưu lại từ khóa trong ô tìm kiếm
                return View(PaginatedList<Video>.Create(videos, pageNumber, pageSize, searchTerm));
            }

            return View(PaginatedList<Video>.Create(new List<Video>(), pageNumber, 1, searchTerm));
        }

        //[Authorize]
        public async Task<IActionResult> Video(string videoId)
        {
            var userId = GetCurrentUserId();

            Video video = new Video();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Video/GetDetails/{videoId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                video = JsonConvert.DeserializeObject<Video>(data);
            }

            // Lấy các vid liên quan
            HttpResponseMessage relatedResponse = await _client.GetAsync(_client.BaseAddress + $"/Video/GetRelatedVideos/{videoId}");
            if (relatedResponse.IsSuccessStatusCode)
            {
                string relatedData = await relatedResponse.Content.ReadAsStringAsync();
                ViewBag.RelatedVideos = JsonConvert.DeserializeObject<List<Video>>(relatedData);
            }

            // Get playlists
            List<Playlist> playlists = new List<Playlist>(); // Khai báo biến playlists ở đây
            if (userId != null)
            {
                HttpResponseMessage playlistResponse = await _client.GetAsync(_client.BaseAddress + $"/Playlist/GetPlaylists/{userId}");
                if (playlistResponse.IsSuccessStatusCode)
                {
                    string playlistData = await playlistResponse.Content.ReadAsStringAsync();
                    playlists = JsonConvert.DeserializeObject<List<Playlist>>(playlistData); // Gán giá trị cho biến playlists ở đây
                }
            }

            // Lấy các comment
            List<Comment> comments = new List<Comment>(); // Khai báo biến comments ở đây
            if (userId != null)
            {
                HttpResponseMessage commentResponse = await _client.GetAsync(_client.BaseAddress + $"/Comment/GetComments/{videoId}");
                if (commentResponse.IsSuccessStatusCode)
                {
                    string commentData = await commentResponse.Content.ReadAsStringAsync();
                    comments = JsonConvert.DeserializeObject<List<Comment>>(commentData); // Gán giá trị cho biến comments ở đây

                    // Load thông tin người dùng cho mỗi comment
                    foreach (var comment in comments)
                    {
                        comment.AppUser.Id = GetCurrentUserId();
                    }
                }
            }

            // Lấy reply comment
            List<ReplyComment> replyComments = new List<ReplyComment>();
            if (userId != null)
            {
                for (int i = 0; i < comments.Count; i++)
                {
                    HttpResponseMessage responseReply = await _client.GetAsync($"{_client.BaseAddress}/ReplyComment/GetReplyComment/{comments[i].CommentId}");
                    if (responseReply.IsSuccessStatusCode)
                    {
                        string replycomment = await responseReply.Content.ReadAsStringAsync();
                        replyComments = JsonConvert.DeserializeObject<List<ReplyComment>>(replycomment);
                        comments[i].Replies = replyComments;

                        foreach (var reply in replyComments)
                        {
                            reply.AppUser.Id = GetCurrentUserId();
                        }
                    }
                }
            }

            // Tạo ViewModel
            var viewModel = new VideoViewModel
            {
                Video = video,
                Playlists = playlists,
                Comments = comments,
                ReplyComments = replyComments
            };

            return View(viewModel);
        }

        public async Task<IActionResult> LikeVideo(string videoId)
        {
            var userId = GetCurrentUserId();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/LikeVideo/{videoId}/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var likes = JsonConvert.DeserializeObject<int>(data);
                return Json(likes); // Trả về số lượt like dưới dạng JSON
            }

            return View();
        }

        public async Task<IActionResult> DislikeVideo(string videoId)
        {
            var userId = GetCurrentUserId();

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/DislikeVideo/{videoId}/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var dislikes = JsonConvert.DeserializeObject<int>(data);
                return Json(dislikes); // Trả về số lượt dislike dưới dạng JSON
            }

            return View();
        }

        public async Task<IActionResult> CheckLikeStatus(string videoId)
        {
            var userId = GetCurrentUserId(); // Lấy userId

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/CheckLikeStatus/{videoId}/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var isLiked = JsonConvert.DeserializeObject<bool>(data);
                return Json(isLiked); // Trả về trạng thái "like" dưới dạng JSON
            }

            return View();
        }

        public async Task<IActionResult> CheckDislikeStatus(string videoId)
        {
            var userId = GetCurrentUserId(); // Lấy userId

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/CheckdislikeStatus/{videoId}/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var isDisliked = JsonConvert.DeserializeObject<bool>(data);
                return Json(isDisliked); // Trả về trạng thái "dislike" dưới dạng JSON
            }

            return View();
        }

        public async Task<IActionResult> UndoLikeVideo(string videoId)
        {
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/UndoLikeVideo/{videoId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var likes = JsonConvert.DeserializeObject<int>(data);
                return Json(likes); // Trả về số lượt like dưới dạng JSON
            }

            return View();
        }

        public async Task<IActionResult> UndoDislikeVideo(string videoId)
        {
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/UndoDislikeVideo/{videoId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var dislikes = JsonConvert.DeserializeObject<int>(data);
                return Json(dislikes); // Trả về số lượt dislike dưới dạng JSON
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> PlaylistDetail(string playlistId)
        {
            var userId = GetCurrentUserId();

            // Lấy các vid trong playlist
            List<Video> videos = new List<Video>();
            if (userId != null && playlistId != null)
            {
                HttpResponseMessage videoResponse = await _client.GetAsync(_client.BaseAddress + $"/Playlist/GetPlaylistVideos/{userId}/{playlistId}");
                if (videoResponse.IsSuccessStatusCode)
                {
                    string videoData = await videoResponse.Content.ReadAsStringAsync();
                    videos = JsonConvert.DeserializeObject<List<Video>>(videoData);
                }
            }

            // Tạo view model
            var viewModel = new PlaylistViewModel
            {
                Videos = videos
            };

            return View(viewModel);
        }

        //[Authorize]
        //public IActionResult MusicDetail()
        //{
        //    return View();
        //}

        [Authorize]
        public async Task<IActionResult> Library()
        {
            var userId = GetCurrentUserId();

            // Get playlists
            List<Playlist> playlists = new List<Playlist>(); // Khai báo biến playlists ở đây
            if (userId != null)
            {
                HttpResponseMessage playlistResponse = await _client.GetAsync(_client.BaseAddress + $"/Playlist/GetPlaylists/{userId}");
                if (playlistResponse.IsSuccessStatusCode)
                {
                    string playlistData = await playlistResponse.Content.ReadAsStringAsync();
                    playlists = JsonConvert.DeserializeObject<List<Playlist>>(playlistData); // Gán giá trị cho biến playlists ở đây
                }
            }

            // Tạo view model
            var viewModel = new PlaylistViewModel
            {
                Playlists = playlists
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Videolike()
        {
            var userId = GetCurrentUserId(); // Lấy userId

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/LikeDislike/GetLikedVideos/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var likedVideos = JsonConvert.DeserializeObject<List<Video>>(data);
                return View(likedVideos); // Trả về view với model là danh sách các video đã like
            }

            return View();
        }

        public async Task<IActionResult> CommentedVideos()
        {
            var userId = GetCurrentUserId(); // Lấy userId

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Video/GetUserCommentedVideos/{userId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var commentedVideos = JsonConvert.DeserializeObject<List<Video>>(data);
                return View(commentedVideos); // Trả về view với model là danh sách các video đã comment
            }

            return View();
        }
    }
}