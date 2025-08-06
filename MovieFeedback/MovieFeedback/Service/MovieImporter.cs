using MovieFeedback.Models;
using Newtonsoft.Json;
using MovieFeedback.DTOs.TMDB;
using Microsoft.EntityFrameworkCore;

namespace MovieFeedback.Service;

public class MovieImporter
{
    private readonly MovieFeedbackDbContext _context;

    public MovieImporter(MovieFeedbackDbContext context)
    {
        _context = context;
    }

    public async Task ImportEverythingAsync(int moviesToImport = 10)
    {
        var importedMovieIds = await SaveMoviesToDatabaseAsync(moviesToImport);
        await ImportGenresAndLinksAsync(importedMovieIds);
        await ImportActorsAndLinksAsync(importedMovieIds);
        await ImportCommentsForMoviesAsync(importedMovieIds);
        await ImportRatingsAccuratelyAsync(importedMovieIds);
        await SeedFavoritesAsync(importedMovieIds);
    }

    public async Task ImportNewMoviesAsync(int maxMovies = 10)
    {
        var newMovieIds = new List<int>();
        int page = 1;
        string minReleaseDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        while (newMovieIds.Count < maxMovies)
        {
            var jsonResponse = await TMDBService.DiscoverMoviesAsync(page, minVotes: 50, minReleaseDate: minReleaseDate);
            var moviesResponse = JsonConvert.DeserializeObject<MoviesResponse>(jsonResponse);

            if (moviesResponse?.Results == null || moviesResponse.Results.Count == 0)
                break;

            foreach (var movie in moviesResponse.Results)
            {
                if (_context.Movies.Any(m => m.TmdbId == movie.Id)) continue;

                var details = await TMDBService.GetMovieDetailsAsync(movie.Id);
                var newMovie = new Movie
                {
                    TmdbId = movie.Id,
                    Title = movie.Title,
                    Description = movie.Overview,
                    ReleaseDate = string.IsNullOrEmpty(movie.ReleaseDate) ? null : DateOnly.FromDateTime(DateTime.Parse(movie.ReleaseDate)),
                    Rating = movie.VoteAverage,
                    PosterPath = movie.PosterPath,
                    Language = movie.OriginalLanguage,
                    Runtime = details?.Runtime,
                    Popularity = movie.Popularity
                };

                _context.Movies.Add(newMovie);
                await _context.SaveChangesAsync();
                newMovieIds.Add(newMovie.MovieId);

                if (newMovieIds.Count >= maxMovies) break;
            }

            page++;
        }

        await ImportGenresAndLinksAsync(newMovieIds);
        await ImportActorsAndLinksAsync(newMovieIds);
        await ImportCommentsForMoviesAsync(newMovieIds);
        await ImportRatingsAccuratelyAsync(newMovieIds);
        await SeedFavoritesAsync(newMovieIds);
    }

    public async Task SeedDefaultPasswordsAsync()
    {
        int usersToUpdateCount = await _context.Users
            .CountAsync(u => u.PasswordHash == "");

        if (usersToUpdateCount == 0)
        {
            Console.WriteLine("Passwords already generated — skipping.");
            return;
        }

        var usersToUpdate = await _context.Users
            .Where(u => u.PasswordHash == "")
            .ToListAsync();

        foreach (var user in usersToUpdate)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("tmdb123");
            Console.WriteLine($"Password was generated for {user.Username} ({user.Email})");
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"Passwords generated for {usersToUpdate.Count} users.");
    }

    private async Task<List<int>> SaveMoviesToDatabaseAsync(int maxMoviesToImport)
    {
        var importedIds = new List<int>();
        int totalImported = 0, currentPage = 1;

        while (totalImported < maxMoviesToImport)
        {
            var jsonResponse = await TMDBService.DiscoverMoviesAsync(currentPage);
            var moviesResponse = JsonConvert.DeserializeObject<MoviesResponse>(jsonResponse);
            if (moviesResponse?.Results == null || moviesResponse.Results.Count == 0) break;

            foreach (var movie in moviesResponse.Results)
            {
                if (_context.Movies.Any(m => m.TmdbId == movie.Id)) continue;

                var details = await TMDBService.GetMovieDetailsAsync(movie.Id);
                var newMovie = new Movie
                {
                    TmdbId = movie.Id,
                    Title = movie.Title,
                    Description = movie.Overview,
                    ReleaseDate = string.IsNullOrEmpty(movie.ReleaseDate) ? null : DateOnly.FromDateTime(DateTime.Parse(movie.ReleaseDate)),
                    Rating = movie.VoteAverage,
                    PosterPath = movie.PosterPath,
                    Language = movie.OriginalLanguage,
                    Runtime = details?.Runtime,
                    Popularity = movie.Popularity
                };

                _context.Movies.Add(newMovie);
                await _context.SaveChangesAsync();
                importedIds.Add(newMovie.MovieId);
                totalImported++;

                if (totalImported >= maxMoviesToImport) break;
            }

            currentPage++;
        }

        return importedIds;
    }

    private async Task ImportGenresAndLinksAsync(List<int> movieIds)
    {
        foreach (var movieId in movieIds)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var details = await TMDBService.GetMovieDetailsAsync(movie.TmdbId);

            foreach (var genreDto in details?.Genres?.Take(6) ?? [])
            {
                var genre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.GenreId == genreDto.GenreId);

                if (genre == null)
                {
                    var genreByName = await _context.Genres
                        .FirstOrDefaultAsync(g => g.Name == genreDto.Name);

                    if (genreByName != null)
                    {
                        Console.WriteLine($"Genre '{genreDto.Name}' already exists.");
                        genre = genreByName;
                    }
                    else
                    {
                        genre = new Genre
                        {
                            Name = genreDto.Name
                        };

                        _context.Genres.Add(genre);
                        await _context.SaveChangesAsync();
                    }
                }

                if (!_context.MovieGenres.Any(mg => mg.MovieId == movie.MovieId && mg.GenreId == genre.GenreId))
                {
                    _context.MovieGenres.Add(new MovieGenre
                    {
                        MovieId = movie.MovieId,
                        GenreId = genre.GenreId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task ImportActorsAndLinksAsync(List<int> movieIds)
    {
        var existingActors = _context.Actors.ToDictionary(a => a.Name, a => a);

        foreach (var movieId in movieIds)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            var actors = (await TMDBService.GetMovieActorsAsync(movie.TmdbId)).Take(8).ToList();

            foreach (var actor in actors)
            {
                if (!existingActors.ContainsKey(actor.Name))
                {
                    _context.Actors.Add(actor);
                    await _context.SaveChangesAsync();
                    existingActors[actor.Name] = actor;
                }

                var actorEntity = existingActors[actor.Name];
                if (!_context.MovieActors.Any(ma => ma.MovieId == movie.MovieId && ma.ActorId == actorEntity.ActorId))
                {
                    _context.MovieActors.Add(new MovieActor
                    {
                        MovieId = movie.MovieId,
                        ActorId = actorEntity.ActorId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task ImportCommentsForMoviesAsync(List<int> movieIds)
    {
        foreach (var movieId in movieIds)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            await SaveCommentsToDatabaseAsync(movie.TmdbId);
        }
    }

    private async Task ImportRatingsAccuratelyAsync(List<int> movieIds)
    {
        var movies = _context.Movies.Where(m => movieIds.Contains(m.MovieId)).ToList();
        var users = _context.Users.ToList();
        var random = new Random();

        foreach (var movie in movies)
        {
            var targetAverage = (double)movie.Rating;
            double popularityFactor = movie.Popularity ?? 0.0;
            int maxRatings = Math.Min((int)Math.Clamp(popularityFactor * 2.0, 20, 30), users.Count);
            if (maxRatings < 5) continue;

            var selectedUsers = users.OrderBy(u => random.Next()).Take(maxRatings).ToList();
            List<int> ratings = new(); double total = 0;

            for (int i = 0; i < maxRatings - 1; i++)
            {
                double deviation = targetAverage >= 8 ? (random.NextDouble() * 2 - 1) : targetAverage >= 6 ? (random.NextDouble() * 2.5 - 1.25) : (random.NextDouble() * 3 - 1.5);
                int value = (int)Math.Round(Math.Max(1, Math.Min(10, targetAverage + deviation)));
                ratings.Add(value); total += value;
            }

            ratings.Add((int)Math.Round(Math.Max(1, Math.Min(10, targetAverage * maxRatings - total))));

            for (int i = 0; i < ratings.Count; i++)
            {
                _context.Ratings.Add(new Rating
                {
                    MovieId = movie.MovieId,
                    UserId = selectedUsers[i].UserId,
                    Rating1 = ratings[i],
                    CreatedAt = DateTime.Now.AddDays(-random.Next(30))
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedFavoritesAsync(List<int> movieIds)
    {
        var users = _context.Users.ToList();
        var random = new Random();

        foreach (var user in users)
        {
            var likedMovieIds = _context.Ratings
                .Where(r => r.UserId == user.UserId && r.Rating1 >= 6 && movieIds.Contains(r.MovieId))
                .Select(r => r.MovieId)
                .ToList();

            var selected = likedMovieIds.OrderBy(x => random.Next()).Take(Math.Min(5, likedMovieIds.Count));
            foreach (var movieId in selected)
            {
                if (!_context.Favorites.Any(f => f.UserId == user.UserId && f.MovieId == movieId))
                {
                    _context.Favorites.Add(new Favorite
                    {
                        UserId = user.UserId,
                        MovieId = movieId,
                        CreatedAt = DateTime.Now.AddDays(-random.Next(30))
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SaveCommentsToDatabaseAsync(int tmdbId)
    {
        var movie = _context.Movies.FirstOrDefault(m => m.TmdbId == tmdbId);
        var comments = await TMDBService.GetMovieCommentsAsync(tmdbId);
        if (movie == null || comments.Count == 0) return;

        var existingContents = _context.Comments.Where(c => c.MovieId == movie.MovieId).Select(c => c.Content).ToHashSet();

        foreach (var comment in comments)
        {
            if (existingContents.Contains(comment.Content)) continue;

            var user = _context.Users.FirstOrDefault(u => u.Username == comment.Author) ?? new User
            {
                Username = comment.Author,
                Email = $"{comment.Author.Replace(" ", "").ToLower()}@tmdb.com",
                PasswordHash = "",
                CreatedAt = DateTime.Now
            };

            if (user.UserId == 0)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            _context.Comments.Add(new Comment
            {
                MovieId = movie.MovieId,
                UserId = user.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            });
        }

        await _context.SaveChangesAsync();
    }
}
