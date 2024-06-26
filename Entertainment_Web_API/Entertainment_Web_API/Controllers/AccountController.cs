using BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;

namespace Entertainment_Web_API.Controllers
{
    public class AccountController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7142/backend");
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
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

        public async Task<IActionResult> Index()
        {
            string currentUserId = GetCurrentUserId();
            AppUser userModel = null;
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/User/Get/{currentUserId}");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                userModel = JsonConvert.DeserializeObject<AppUser>(data);
            }

            return View(userModel);
        }
    }
}