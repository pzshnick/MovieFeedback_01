using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieFeedback.Controllers;
using MovieFeedback.DTOs.Stats;
using MovieFeedback.Models;
using MovieFeedback.ViewModels;
using System.Linq;

namespace MovieFeedback.Service
{
    public interface IStatsService
    {
        Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId, DateTime fromDate, DateTime toDate);
        Task<List<RatingHistoryPoint>> GetRatingsHistoryAsync(int userId, DateTime? fromDate, DateTime? toDate);
        Task<List<ActivityLevelDto>> GetFavoriteMoviesAsync(int userId, DateTime? fromDate, DateTime? toDate);
        Task<List<ActivityLevelDto>> GetActivityLevelAsync(int userId, DateTime? fromDate, DateTime? toDate);
        Task<AverageRatingDto> GetAverageRatingAsync(int userId, DateTime? fromDate, DateTime? toDate);

    }

    public class StatsService : IStatsService
    {
        private readonly MovieFeedbackDbContext _context;

        public StatsService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            var ratings = await _context.Ratings
                .Where(r => r.UserId == userId && r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
                .ToListAsync();
            
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId && f.CreatedAt >= fromDate && f.CreatedAt <= toDate)
                .ToListAsync();

            return new UserStatisticsViewModel
            {
                TotalRatings = ratings.Count,
                AverageRating = ratings.Any() ? ratings.Average(r => r.Rating1 ?? 0) : 0,
                TotalFavorites = favorites.Count
            };
        }

        public async Task<List<RatingHistoryPoint>> GetRatingsHistoryAsync(int userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Ratings
                .Where(r => r.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.CreatedAt <= toDate.Value);

            var grouped = await query
                .GroupBy(r => r.CreatedAt.Value.Date)
                .Select(g => new RatingHistoryPoint
                {
                    Date = g.Key,
                    AverageRating = g.Average(r => r.Rating1 ?? 0)
                })
                .OrderBy(p => p.Date)
                .ToListAsync();

            return grouped;
        }

        public async Task<List<ActivityLevelDto>> GetFavoriteMoviesAsync(int userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Favorites
                .Where(f => f.UserId == userId);

            if (fromDate.HasValue && toDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt >= fromDate && f.CreatedAt <= toDate);
            }

            return await query
                .GroupBy(f => f.CreatedAt.Date)
                .Select(g => new ActivityLevelDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<ActivityLevelDto>> GetActivityLevelAsync(int userId, DateTime? fromDate, DateTime? toDate)
        {
            var ratingDatesQuery = _context.Ratings
                .Where(r => r.UserId == userId)
                .Select(r => r.CreatedAt.Value);

            var favoriteDatesQuery = _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.CreatedAt);

            var activitiesQuery = ratingDatesQuery.Union(favoriteDatesQuery);

            if (fromDate.HasValue && toDate.HasValue)
            {
                activitiesQuery = activitiesQuery
                    .Where(d => d >= fromDate.Value && d <= toDate.Value);
            }

            var activities = await activitiesQuery.ToListAsync();

            return activities
                .GroupBy(d => new { d.Year, d.Month })
                .Select(g => new ActivityLevelDto
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        public async Task<AverageRatingDto> GetAverageRatingAsync(int userId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Ratings
                .Where(r => r.UserId == userId);

            if (fromDate.HasValue && toDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate);
            }

            var avg = await query.AverageAsync(r => (double?)r.Rating1) ?? 0;

            return new AverageRatingDto
            {
                AverageRating = avg
            };
        }

    }

}
