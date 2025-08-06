namespace MovieFeedback.DTOs.TMDB
{
    public class TMDBRecommendationResponse
    {
        public List<TMDBRecommendationDto> Results { get; set; }
    }

    public class TMDBRecommendationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public double VoteAverage { get; set; }
    }

}
