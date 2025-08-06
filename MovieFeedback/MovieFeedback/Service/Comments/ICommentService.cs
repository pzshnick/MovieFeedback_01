namespace MovieFeedback.Service.Comments
{
    public interface ICommentService
    {
        Task AddCommentAsync(int movieId, int userId, string content);
        Task EditCommentAsync(int commentId, int userId, string newContent);
        Task DeleteCommentAsync(int commentId, int userId);
        Task<int> GetMovieIdByCommentIdAsync(int commentId);
    }

}
