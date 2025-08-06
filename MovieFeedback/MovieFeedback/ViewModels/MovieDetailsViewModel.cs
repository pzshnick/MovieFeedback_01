namespace MovieFeedback.ViewModels
{
    public class MovieDetailsViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterPath { get; set; }
        public double Rating { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public int Runtime { get; set; }
        public string Language { get; set; }

        public List<string> Genres { get; set; } = new();
        public List<string> Actors { get; set; } = new();

        public int? UserRating { get; set; }
        public bool IsFavorite { get; set; }

        public List<CommentViewModel> Comments { get; set; } = new();
    }

    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
        public int Level { get; set; } = 0;

    }

}
