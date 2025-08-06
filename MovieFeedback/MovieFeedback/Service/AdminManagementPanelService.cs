using Microsoft.EntityFrameworkCore;
using MovieFeedback.Models;
using MovieFeedback.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieFeedback.Services
{
    public class AdminManagementPanelService : IAdminManagementPanelService
    {
        private readonly MovieFeedbackDbContext _context;

        public AdminManagementPanelService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovieManagementViewModel>> GetMoviesForManagementAsync(string searchQuery, int? genreId, double? minRating)
        {
            var query = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(m => m.Title.Contains(searchQuery));
            }

            if (genreId.HasValue)
            {
                query = query.Where(m => m.MovieGenres.Any(g => g.GenreId == genreId.Value));
            }

            if (minRating.HasValue)
            {
                query = query.Where(m => m.Rating >= minRating.Value);
            }

            return await query
                .Select(m => new MovieManagementViewModel
                {
                    MovieId = m.MovieId,
                    Title = m.Title,
                    Rating = m.Rating ?? 0,
                    ReleaseDate = m.ReleaseDate
                })
                .ToListAsync();
        }

        public async Task<MovieEditViewModel> GetMovieDetailsForEditAsync(int movieId)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.MovieId == movieId);

            if (movie == null)
                return null;

            return new MovieEditViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList() 
            };
        }


        public async Task<bool> UpdateMovieAsync(MovieEditViewModel model)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.MovieId == model.MovieId);

            if (movie == null)
                return false;

            movie.Title = model.Title;
            movie.Description = model.Description;

            if (model.Genres != null)
            {
                _context.MovieGenres.RemoveRange(movie.MovieGenres);

                movie.MovieGenres = model.Genres.Select(genreName => new MovieGenre
                {
                    MovieId = movie.MovieId,
                    GenreId = _context.Genres.First(g => g.Name == genreName).GenreId
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteMovieAsync(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetAllGenresAsync()
        {
            return await _context.Genres
                                 .OrderBy(g => g.Name)
                                 .Select(g => g.Name)
                                 .ToListAsync();
        }

        public async Task<List<ManageUserViewModel>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new ManageUserViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsBanned = u.IsBanned
                }).ToListAsync();
        }

        public async Task UpdateUserRoleAsync(int userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            user.Role = role;
            await _context.SaveChangesAsync();
        }

        public async Task ToggleUserBanAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            user.IsBanned = !user.IsBanned;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

    }
}
