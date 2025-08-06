namespace MovieFeedback.ViewModels
{
    public class ManageMoviesPageViewModel
    {
        public List<MovieManagementViewModel> Movies { get; set; }
        public int CurrentPage { get; set; }
        public int TotalMovies { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
    }
}
