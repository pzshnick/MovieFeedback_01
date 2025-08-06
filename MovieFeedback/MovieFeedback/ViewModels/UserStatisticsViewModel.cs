using Microsoft.AspNetCore.Mvc;

namespace MovieFeedback.ViewModels
{
    public class UserStatisticsViewModel
    {
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public int TotalFavorites { get; set; }
    }

}
