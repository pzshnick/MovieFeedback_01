using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieFeedback.Service.Comments;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MovieFeedback.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _commentService.EditCommentAsync(commentId, userId, content);
            var movieId = await _commentService.GetMovieIdByCommentIdAsync(commentId);
            TempData["SuccessMessage"] = "Comment updated successfully!";
            return RedirectToAction("Details", "Movie", new { id = movieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var movieId = await _commentService.GetMovieIdByCommentIdAsync(commentId);
            TempData["SuccessMessage"] = "Comment deleted successfully!";
            await _commentService.DeleteCommentAsync(commentId, userId);
            return RedirectToAction("Details", "Movie", new { id = movieId });
        }
    }
}
