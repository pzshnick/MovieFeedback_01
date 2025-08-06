namespace MovieFeedback.Service.Ratings
{
    public interface IRatingService
    {
        Task AddOrUpdateRatingAsync(int movieId, int userId, int rating);
    }

}
