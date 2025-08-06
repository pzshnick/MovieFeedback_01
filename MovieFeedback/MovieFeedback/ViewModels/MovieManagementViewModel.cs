namespace MovieFeedback.ViewModels
{
    public class MovieManagementViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public double Rating { get; set; }
        public DateOnly? ReleaseDate { get; set; }
    }
}
