using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MovieFeedback.Models;
using MovieFeedback.Services;
using MovieFeedback.ViewModels;

namespace MovieFeedback.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieSearchService _movieSearchService;

        public HomeController(ILogger<HomeController> logger, IMovieSearchService movieSearchService)
        {
            _logger = logger;
            _movieSearchService = movieSearchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoleClaim = User.FindFirstValue(ClaimTypes.Role);

            var model = new HomePageViewModel();

            var userId = int.Parse(userIdClaim);

            if (userRoleClaim != null && userRoleClaim != "Visitor")
            {
                model.RecentRatings = await _movieSearchService.GetRecentRatingsAsync(userId);
                model.Recommendations = new List<MovieShortViewModel>();
                //model.Recommendations = await _movieSearchService.GetRecommendationsAsync(userId);
            }
            else
            {
                model.PopularMovies = await _movieSearchService.GetPopularMoviesAsync();
            }

            return View(model);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
