using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieFeedback.Models;
using MovieFeedback.Service;
using System.Security.Claims;

[Authorize]
public class StatsController : Controller
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public IActionResult Statistics()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Statistics(DateTime fromDate, DateTime toDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var stats = await _statsService.GetUserStatisticsAsync(userId, fromDate, toDate);
        return View(stats);
    }
    [HttpGet]
    public async Task<IActionResult> GetRatingsData(DateTime? fromDate, DateTime? toDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var data = await _statsService.GetRatingsHistoryAsync(userId, fromDate, toDate);

        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetFavoritesData(DateTime? fromDate, DateTime? toDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var data = await _statsService.GetFavoriteMoviesAsync(userId, fromDate, toDate);
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityData(DateTime? fromDate, DateTime? toDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var data = await _statsService.GetActivityLevelAsync(userId, fromDate, toDate);
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAverageRating(DateTime? fromDate, DateTime? toDate)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var avgRating = await _statsService.GetAverageRatingAsync(userId, fromDate, toDate);
        return Json(avgRating);
    }
}
