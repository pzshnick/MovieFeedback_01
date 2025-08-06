using MovieFeedback.ViewModels;

namespace MovieFeedback.Services
{
    public interface IAdminManagementPanelService
    {
        Task<List<MovieManagementViewModel>> GetMoviesForManagementAsync(string searchQuery, int? genreId, double? minRating);
        Task<MovieEditViewModel> GetMovieDetailsForEditAsync(int movieId);
        Task<bool> UpdateMovieAsync(MovieEditViewModel model);
        Task<List<string>> GetAllGenresAsync();
        Task<bool> DeleteMovieAsync(int movieId);
        Task<List<ManageUserViewModel>> GetAllUsersAsync();
        Task UpdateUserRoleAsync(int userId, string role);
        Task ToggleUserBanAsync(int userId);
        Task DeleteUserAsync(int userId);
    }
}