namespace MovieFeedback.ViewModels
{
    public class FavoriteMovieViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public double Rating { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public List<string> Genres { get; set; }
        public bool IsWatched { get; set; }

    }

}
