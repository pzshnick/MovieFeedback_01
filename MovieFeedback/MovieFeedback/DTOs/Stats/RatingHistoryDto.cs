using Microsoft.AspNetCore.Mvc;

namespace MovieFeedback.DTOs.Stats
{
    public class RatingHistoryDto
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
    }
}
