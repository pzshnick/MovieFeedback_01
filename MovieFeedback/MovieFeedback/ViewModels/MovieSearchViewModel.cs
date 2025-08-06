using MovieFeedback.Models;

namespace MovieFeedback.ViewModels
{
    public class MovieSearchViewModel
    {
        public string Query { get; set; }
        public int? GenreId { get; set; }
        public double? MinRating { get; set; }
        public int? ReleaseYear { get; set; }
        public bool OnlyFavorites { get; set; }

        public List<Movie> Results { get; set; } = new();
        public List<Genre> Genres { get; set; } = new();
        public string? SortOrder { get; set; }

        public int CurrentPage { get; set; }
        public int TotalResults { get; set; }
    }
}
