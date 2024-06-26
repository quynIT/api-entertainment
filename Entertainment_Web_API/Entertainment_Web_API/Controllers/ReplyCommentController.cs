using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Entertainment_Web_API.Controllers
{
    public class ReplyCommentController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7142/backend");
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReplyCommentController(IHttpContextAccessor httpContextAccessor)
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
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> AddReplyComment(string commentId, string content)
        {
            var userId = GetCurrentUserId();
            var replycomment = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userId", userId),
                new KeyValuePair<string, string>("content", content),
                new KeyValuePair<string, string>("commentId", commentId)

            });
            HttpResponseMessage response = await _client.PostAsync($"{_client.BaseAddress}/ReplyComment/AddReplyComment/{commentId}/{userId}/{content}", replycomment);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Add reply comment fail!" });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateReplyComment(string replyId, string content)
        {
            var replycomment = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("replyId", replyId),
                new KeyValuePair<string, string>("content", content)
            });

            HttpResponseMessage respone = await _client.PutAsync($"{_client.BaseAddress}/ReplyComment/UpdateReplyComment/{replyId}/{content}", replycomment);
            if (respone.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Update reply comment fail!" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReplyComment(string replyId)
        {
            HttpResponseMessage httpResponse = await _client.DeleteAsync($"{_client.BaseAddress}/ReplyComment/DeleteReplyComment/{replyId}");

            if (httpResponse.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Delete reply comment fail!" });

        }
    }
}