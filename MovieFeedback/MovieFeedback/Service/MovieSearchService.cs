using Microsoft.EntityFrameworkCore;
using MovieFeedback.Models;
using MovieFeedback.Service;
using MovieFeedback.ViewModels;
using System.Security.Claims;

namespace MovieFeedback.Services
{
    public interface IMovieSearchService
    {
        Task<MovieSearchViewModel> SearchMoviesAsync(
            string query, int? genreId, double? minRating, int? releaseYear,
            bool onlyFavorites, ClaimsPrincipal user, int page, string? sortOrder, string? language);
        Task<MovieDetailsViewModel> GetMovieDetailsAsync(int movieId, ClaimsPrincipal user);
        Task<List<FavoriteMovieViewModel>> GetFavoriteMoviesAsync(int userId);
        Task RemoveFromFavoritesAsync(int userId, int movieId);
        Task<List<RecentRatingViewModel>> GetRecentRatingsAsync(int userId, int count = 5);
        Task<List<MovieShortViewModel>> GetRecommendationsAsync(int userId, int count = 8);
        Task<List<MovieShortViewModel>> GetPopularMoviesAsync();
        Task AddReplyAsync(int movieId, int parentCommentId, int userId, string content);
    }

    public class MovieSearchService : IMovieSearchService
    {
        private readonly MovieFeedbackDbContext _context;
        private const int PageSize = 10;

        public MovieSearchService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task<MovieSearchViewModel> SearchMoviesAsync(
            string query, int? genreId, double? minRating, int? releaseYear,
            bool onlyFavorites, ClaimsPrincipal user, int page, string? sortOrder, string? language)
        {
            var movies = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(g => g.Genre)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                movies = movies.Where(m =>
                    m.Title.Contains(query) ||
                    m.MovieGenres.Any(g => g.Genre.Name.Contains(query)) ||
                    m.MovieActors.Any(a => a.Actor.Name.Contains(query))
                );
            }

            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(g => g.GenreId == genreId.Value));

            if (minRating.HasValue)
                movies = movies.Where(m => m.Rating >= minRating.Value);

            if (releaseYear.HasValue)
            {
                var start = new DateOnly(releaseYear.Value, 1, 1);
                var end = new DateOnly(releaseYear.Value, 12, 31);
                movies = movies.Where(m => m.ReleaseDate >= start && m.ReleaseDate <= end);
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                movies = movies.Where(m => m.Language == language);
            }


            if (onlyFavorites && user.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
                movies = movies.Where(m => m.Favorites.Any(f => f.UserId == userId));
            }

            movies = sortOrder switch
            {
                "rating_desc" => movies.OrderByDescending(m => m.Rating),
                "rating_asc" => movies.OrderBy(m => m.Rating),
                "title_asc" => movies.OrderBy(m => m.Title),
                "title_desc" => movies.OrderByDescending(m => m.Title),
                "newest" => movies.OrderByDescending(m => m.ReleaseDate),
                "oldest" => movies.OrderBy(m => m.ReleaseDate),
                _ => movies.OrderByDescending(m => m.Rating)
            };

            var totalResults = await movies.CountAsync();

            var result = await movies
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new MovieSearchViewModel
            {
                Query = query,
                GenreId = genreId,
                MinRating = minRating,
                ReleaseYear = releaseYear,
                OnlyFavorites = onlyFavorites,
                SortOrder = sortOrder,
                Results = result,
                Genres = await _context.Genres.ToListAsync(),
                CurrentPage = page,
                TotalResults = totalResults
            };
        }

        public async Task<MovieDetailsViewModel> GetMovieDetailsAsync(int movieId, ClaimsPrincipal user)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(g => g.Genre)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.Comments).ThenInclude(c => c.User)
                .Include(m => m.Favorites)
                .FirstOrDefaultAsync(m => m.MovieId == movieId);

            if (movie == null)
                return null;

            var allComments = movie.Comments
                .Select(c => new CommentViewModel
                {
                    CommentId = c.CommentId,
                    Username = c.User.Username,
                    Content = c.Content,
                    PostedAt = c.CreatedAt ?? default,
                    ParentCommentId = c.ParentCommentId
                })
                .ToList();

            // Побудова ієрархії
            var commentDict = allComments.ToDictionary(c => c.CommentId);

            foreach (var comment in allComments)
            {
                if (comment.ParentCommentId.HasValue)
                {
                    if (commentDict.TryGetValue(comment.ParentCommentId.Value, out var parent))
                    {
                        parent.Replies.Add(comment);
                    }
                }
            }

            var viewModel = new MovieDetailsViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                PosterPath = movie.PosterPath,
                Rating = movie.Rating ?? 0,
                ReleaseDate = movie.ReleaseDate ?? default,
                Runtime = movie.Runtime ?? 0,
                Language = movie.Language,
                Genres = movie.MovieGenres.Select(g => g.Genre.Name).ToList(),
                Actors = movie.MovieActors.Select(a => a.Actor.Name).ToList(),
                Comments = commentDict.Values.Where(c => c.ParentCommentId == null).ToList()
            };

            int? userId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            if (userId.HasValue)
            {
                var rating = await _context.Ratings
                    .Where(r => r.MovieId == movieId && r.UserId == userId.Value)
                    .Select(r => (int?)r.Rating1)
                    .FirstOrDefaultAsync();

                var isFavorite = await _context.Favorites
                    .AnyAsync(f => f.MovieId == movieId && f.UserId == userId.Value);

                viewModel.UserRating = rating;
                viewModel.IsFavorite = isFavorite;
            }

            return viewModel;
        }

        public async Task<List<FavoriteMovieViewModel>> GetFavoriteMoviesAsync(int userId)
        {
            var ratedMovieIds = await _context.Ratings
                .Where(r => r.UserId == userId)
                .Select(r => r.MovieId)
                .ToListAsync();

            var ratedMovieIdsSet = new HashSet<int>(ratedMovieIds);
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => new FavoriteMovieViewModel
                {
                    MovieId = f.Movie.MovieId,
                    Title = f.Movie.Title,
                    PosterPath = f.Movie.PosterPath,
                    Rating = (double)f.Movie.Rating,
                    ReleaseDate = (DateOnly)f.Movie.ReleaseDate,
                    Genres = f.Movie.MovieGenres.Select(g => g.Genre.Name).ToList(),
                    IsWatched = false
                })
                .ToListAsync();

            foreach (var favorite in favorites)
            {
                if (ratedMovieIdsSet.Contains(favorite.MovieId))
                {
                    favorite.IsWatched = true;
                }
            }

            return favorites;
        }

        public async Task RemoveFromFavoritesAsync(int userId, int movieId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RecentRatingViewModel>> GetRecentRatingsAsync(int userId, int count = 5)
        {
            return await _context.Ratings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .Select(r => new RecentRatingViewModel
                {
                    MovieTitle = r.Movie.Title,
                    Rating = (int)r.Rating1,
                    RatedAt = r.CreatedAt ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<List<MovieShortViewModel>> GetRecommendationsAsync(int userId, int count = 6)
        {
            var favoriteTmdbIds = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Movie.TmdbId)
                .ToListAsync();

            if (favoriteTmdbIds == null || favoriteTmdbIds.Count == 0)
                return new List<MovieShortViewModel>();

            var random = new Random();
            var shuffledFavorites = favoriteTmdbIds.OrderBy(x => random.Next()).ToList();

            foreach (var favoriteTmdbId in shuffledFavorites)
            {
                var recommendedFromApi = await TMDBService.GetRecommendedMoviesAsync(favoriteTmdbId);

                var tmdbIds = recommendedFromApi.Select(r => r.Id).ToList();

                var existingMovies = await _context.Movies
                    .Where(m => tmdbIds.Contains(m.TmdbId))
                    .Select(m => new MovieShortViewModel
                    {
                        MovieId = m.MovieId,
                        Title = m.Title,
                        PosterPath = m.PosterPath,
                        Rating = m.Rating ?? 0
                    })
                    .ToListAsync();

                if (existingMovies.Count >= count)
                {
                    return existingMovies.Take(count).ToList();
                }
            }

            return new List<MovieShortViewModel>();
        }

        public async Task<List<MovieShortViewModel>> GetPopularMoviesAsync()
        {
            return await _context.Movies
                .OrderByDescending(m => m.Popularity)
                .Take(12)
                .Select(m => new MovieShortViewModel
                {
                    MovieId = m.MovieId,
                    Title = m.Title,
                    PosterPath = m.PosterPath
                })
                .ToListAsync();
        }

        public async Task AddReplyAsync(int movieId, int parentCommentId, int userId, string content)
        {
            var reply = new Comment
            {
                MovieId = movieId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();
        }


    }
}
