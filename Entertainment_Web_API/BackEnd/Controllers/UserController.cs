using BackEnd.Data;
using BackEnd.Models;
using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackEnd.Controllers
{
	[Route("backend/[controller]/[action]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly EntertainmentContext _context;

		public UserController(EntertainmentContext context)
		{
			_context = context;
		}


		[HttpGet("{Id}")]
		public IActionResult Get(string Id)
		{
			var user = _context.AppUser.FirstOrDefault(u => u.Id == Id);
			if (user == null)
			{
				return NotFound(); // 404 Not Found nếu không tìm thấy người dùng
			}

			try
			{
				var userModel = new AppUser
				{
					Id = user.Id,
					FullName = user.FullName,
					UserNamee = user.UserNamee,
					PhoneNumber = user.PhoneNumber,
					Email = user.Email,
					Avtprofile = user.Avtprofile
				};
				return Ok(userModel);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi khi truy xuất dữ liệu: {ex.Message}");
			}
		}
	}
}