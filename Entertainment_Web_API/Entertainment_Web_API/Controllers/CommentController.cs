using Microsoft.AspNetCore.Mvc;
using BackEnd.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using Google.Apis.YouTube.v3.Data;

namespace Entertainment_Web_API.Controllers
{
    public class CommentController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7142/backend");
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentController(IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> AddComment(string videoId, string commentContent)
        {
            var userId = GetCurrentUserId();

            // Tạo một đối tượng chứa dữ liệu cần gửi, nó lấy theo dạng form
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userId", userId),
                new KeyValuePair<string, string>("videoId", videoId),
                new KeyValuePair<string, string>("content", commentContent)
            });

            // Gửi yêu cầu POST đến Web API
            HttpResponseMessage response = await _client.PostAsync($"{_client.BaseAddress}/Comment/AddComment/{userId}/{videoId}/{commentContent}", content);
            if (response.IsSuccessStatusCode)
            {
                // Nếu thành công, trả về thông báo thành công
                return Json(new { success = true });
            }
            else
            {
                // Nếu không thành công, trả về thông báo lỗi
                return Json(new { success = false, message = "Adding comment failed!" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateComment(string commentId, string commentContent)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("commentId", commentId),
                new KeyValuePair<string, string>("content", commentContent)
            });

            HttpResponseMessage respone = await _client.PutAsync($"{_client.BaseAddress}/Comment/UpdateComment/{commentId}/{commentContent}", content);
            if (respone.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Edit comment failed" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            HttpResponseMessage response = await _client.DeleteAsync($"{_client.BaseAddress}/Comment/DeleteComment/{commentId}");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Delete comment failed!" });
            }
        }
    }
}
