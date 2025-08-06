using MovieFeedback.Models;
using System.Transactions;
using Microsoft.EntityFrameworkCore;


namespace MovieFeedback.Service.Ratings
{
    public class RatingService : IRatingService
    {
        private readonly MovieFeedbackDbContext _context;

        public RatingService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task AddOrUpdateRatingAsync(int movieId, int userId, int rating)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.Rating1 = rating;
                    existingRating.CreatedAt = DateTime.UtcNow;
                    _context.Ratings.Update(existingRating);
                }
                else
                {
                    _context.Ratings.Add(new Rating
                    {
                        MovieId = movieId,
                        UserId = userId,
                        Rating1 = rating,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("EXEC UpdateMovieRatings @p0", movieId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}
