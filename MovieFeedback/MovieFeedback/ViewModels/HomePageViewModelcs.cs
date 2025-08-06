namespace MovieFeedback.ViewModels
{
    public class HomePageViewModel
    {
        public List<RecentRatingViewModel> RecentRatings { get; set; }
        public List<MovieShortViewModel> Recommendations { get; set; } = new();

        public List<MovieShortViewModel> PopularMovies { get; set; } = new();
    }

}
