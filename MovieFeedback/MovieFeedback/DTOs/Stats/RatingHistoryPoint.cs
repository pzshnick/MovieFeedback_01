using Microsoft.AspNetCore.Mvc;

namespace MovieFeedback.DTOs.Stats
{
    public class RatingHistoryPoint
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
    }
}
