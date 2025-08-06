namespace MovieFeedback.Service.Favorites
{
    public interface IFavoriteService
    {
        Task ToggleFavoriteAsync(int movieId, int userId);
    }

}
