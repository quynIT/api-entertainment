using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Entertainment_Web_API.Controllers
{
    public class PlaylistController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7142/backend");
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlaylistController(IHttpContextAccessor httpContextAccessor)
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

        [HttpPost]
        public async Task<IActionResult> AddVideoToPlaylist(string playlistId, string videoId)
        {
            // Tạo một đối tượng chứa dữ liệu cần gửi
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("playlistId", playlistId),
                new KeyValuePair<string, string>("videoId", videoId)
            });

            // Gửi yêu cầu POST đến Web API
            HttpResponseMessage response = await _client.PostAsync($"{_client.BaseAddress}/Playlist/AddVideoToPlaylist/{playlistId}/{videoId}", content);
            if (response.IsSuccessStatusCode)
            {
                // Nếu thành công, trả về thông báo thành công
                return Json(new { success = true, message = "Added video to playlist successfully!" });
            }
            else
            {
                // Nếu không thành công, trả về thông báo lỗi
                return Json(new { success = false, message = "Video already exists in the playlist!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist(string videoId, string playlistName)
        {
            var userId = GetCurrentUserId();

            // Tạo một đối tượng chứa dữ liệu cần gửi, nó lấy theo dạng form
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userId", userId),
                new KeyValuePair<string, string>("videoId", videoId),
                new KeyValuePair<string, string>("playlistName", playlistName)
            });

            // Gửi yêu cầu POST đến Web API
            HttpResponseMessage response = await _client.PostAsync($"{_client.BaseAddress}/Playlist/CreatePlaylist/{userId}/{videoId}/{playlistName}", content);
            if (response.IsSuccessStatusCode)
            {
                // Nếu thành công, trả về thông báo thành công
                return Json(new { success = true, message = "Playlist added successfully!" });
            }
            else
            {
				// Nếu không thành công, trả về thông báo lỗi
				var errorResponse = await response.Content.ReadAsStringAsync();
				return Json(new { success = false, message = $"Adding playlist failed! Error: {errorResponse}" });
			}
        }

        [HttpPut]
        public async Task<IActionResult> EditPlaylist(string playlistId, string playlistName)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("playlistId", playlistId),
                new KeyValuePair<string, string>("playlistName", playlistName)
            });

            HttpResponseMessage respone = await _client.PutAsync($"{_client.BaseAddress}/Playlist/EditPlaylist/{playlistId}/{playlistName}", content);
            if (respone.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Playlist edit successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Edit playlist failed"});
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePlaylist(string playlistId)
        {
            HttpResponseMessage response = await _client.DeleteAsync($"{_client.BaseAddress}/Playlist/DeletePlaylist/{playlistId}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Playlist deleted successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Delete playlist failed" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteVideoFromPlaylist(string playlistId, string videoId)
        {
            HttpResponseMessage response = await _client.DeleteAsync($"{_client.BaseAddress}/Playlist/DeleteVideoFromPlaylist/{playlistId}/{videoId}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Video delete in playlist successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "Delete video in playlist failed!" });
            }
        }
    }
}
