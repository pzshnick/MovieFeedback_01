namespace MovieFeedback.ViewModels
{
    public class ManageUsersPageViewModel
    {
        public List<ManageUserViewModel> Users { get; set; }
        public int TotalUsers { get; set; }
        public int CurrentPage { get; set; }
        public string? SearchQuery { get; set; }
    }

}
