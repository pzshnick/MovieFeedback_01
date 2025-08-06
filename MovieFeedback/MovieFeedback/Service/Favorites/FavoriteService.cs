using MovieFeedback.Models;
using Microsoft.EntityFrameworkCore;

namespace MovieFeedback.Service.Favorites
{
    public class FavoriteService : IFavoriteService
    {
        private readonly MovieFeedbackDbContext _context;

        public FavoriteService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task ToggleFavoriteAsync(int movieId, int userId)
        {
            var existing = await _context.Favorites.FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    MovieId = movieId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }

}
