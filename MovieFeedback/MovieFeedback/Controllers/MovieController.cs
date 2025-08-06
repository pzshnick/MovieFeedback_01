using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieFeedback.Service.Comments;
using MovieFeedback.Service.Export.MovieFeedback.Helpers;
using MovieFeedback.Service.Favorites;
using MovieFeedback.Service.Ratings;
using MovieFeedback.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MovieFeedback.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieSearchService _movieSearchService;

        private readonly ICommentService _commentService;
        private readonly IRatingService _ratingService;
        private readonly IFavoriteService _favoriteService;

        public MovieController(IMovieSearchService movieSearchService, ICommentService commentService, IRatingService ratingService, IFavoriteService favoriteService)
        {
            _movieSearchService = movieSearchService;
            _commentService = commentService;
            _ratingService = ratingService;
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, int? genreId, double? minRating, int? releaseYear, bool onlyFavorites = false, int page = 1, string? sortOrder = null, string? language = null)
        {
            var model = await _movieSearchService.SearchMoviesAsync(query, genreId, minRating, releaseYear, onlyFavorites, User, page, sortOrder, language);
            return View("SearchResults", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieSearchService.GetMovieDetailsAsync(id, User);

            if (movie == null)
                return NotFound();

            return View("MovieDetails", movie);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var favorites = await _movieSearchService.GetFavoriteMoviesAsync(userId);
            return View("Favorites", favorites);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(int movieId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _movieSearchService.RemoveFromFavoritesAsync(userId, movieId);
            TempData["SuccessMessage"] = "Favorite updated successfully!";
            return RedirectToAction("Favorites");
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int movieId, string content)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _commentService.AddCommentAsync(movieId, userId, content);
            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("Details", new { id = movieId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int movieId, int rating)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _ratingService.AddOrUpdateRatingAsync(movieId, userId, rating);
            TempData["SuccessMessage"] = "Rating submitted successfully!";
            return RedirectToAction("Details", new { id = movieId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _favoriteService.ToggleFavoriteAsync(movieId, userId);
            TempData["SuccessMessage"] = "Favorite updated successfully!";
            return RedirectToAction("Details", new { id = movieId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportFavorites(string format)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var favorites = await _movieSearchService.GetFavoriteMoviesAsync(userId);

            if (favorites == null || !favorites.Any())
                return NotFound("No favorites to export.");

            byte[] fileBytes;
            string contentType;
            string fileName;

            switch (format.ToLower())
            {
                case "csv":
                    fileBytes = ExportHelpers.GenerateCsv(favorites);
                    contentType = "text/csv";
                    fileName = "favorites.csv";
                    break;
                case "xml":
                    fileBytes = ExportHelpers.GenerateXml(favorites);
                    contentType = "application/xml";
                    fileName = "favorites.xml";
                    break;
                case "json":
                    fileBytes = ExportHelpers.GenerateJson(favorites);
                    contentType = "application/json";
                    fileName = "favorites.json";
                    break;
                case "rdf":
                    fileBytes = ExportHelpers.GenerateRdf(favorites);
                    contentType = "application/xml";
                    fileName = "favorites.rdf";
                    break;
                default:
                    return BadRequest("Unsupported format.");
            }

            return File(fileBytes, contentType, fileName);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyComment(int movieId, int parentCommentId, string content)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _movieSearchService.AddReplyAsync(movieId, parentCommentId, userId, content);

            TempData["SuccessMessage"] = "Reply added successfully!";
            return RedirectToAction("Details", new { id = movieId });
        }


    }
}
