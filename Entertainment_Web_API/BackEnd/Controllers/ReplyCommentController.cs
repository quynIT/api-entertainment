using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [Route("backend/[controller]/[action]")]
    [ApiController]
    public class ReplyCommentController : ControllerBase
    {
        private readonly EntertainmentContext _context;
        private readonly string apiKey = "AIzaSyAHK-ZURhPgkkphHFT1szmPr6Dhx_zYH1M";

        public ReplyCommentController(EntertainmentContext context)
        {
            _context = context;
        }

        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetReplyComment(string commentId)
        {
            var replycomment = await _context.ReplyComments
                .Include(c => c.AppUser)
                .Include(c => c.Comment)
                .Where(c => c.CommentId == commentId).ToListAsync();

            if (replycomment == null)
            {
                return NotFound("Reply comment not found");
            }

            return Ok(replycomment);
        }

        [HttpPost("{commentId}/{userId}/{content}")]
        public async Task<IActionResult> AddReplyComment(string commentId, string userId, string content)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _context.Comments.Include(c => c.Replies).FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null)
            {
                return BadRequest("Comment not found");
            }

            var replycomment = new ReplyComment
            {
                ReplyId = Guid.NewGuid().ToString(),
                ReplyContent = content,
                ReplyPostingTime = DateOnly.FromDateTime(DateTime.UtcNow),
                UserId = userId,
                CommentId = commentId
            };

            comment.Replies.Add(replycomment);
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                return BadRequest("Save fail");
            }

            return Ok(replycomment);
        }

        [HttpPut("{replyId}/{content}")]
        public async Task<IActionResult> UpdateReplyComment(string replyId, string content)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var replycomment = await _context.ReplyComments.FirstOrDefaultAsync(r => r.ReplyId == replyId);

            if (replycomment == null)
            {
                return NotFound("Reply comment not found");
            }

            replycomment.ReplyContent = content;
            await _context.SaveChangesAsync();

            return Ok(replycomment);
        }

        [HttpDelete("{replyId}")]
        public async Task<IActionResult> DeleteReplyComment(string replyId)
        {
            var replycomment = await _context.ReplyComments.FirstOrDefaultAsync(r => r.ReplyId == replyId);
            if (replycomment == null)
            {
                return NotFound();
            }

            _context.ReplyComments.Remove(replycomment);
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                return BadRequest("save fail!");
            }

            return Ok();
        }
    }
}