using Microsoft.EntityFrameworkCore;
using MovieFeedback.Models;
using System.Transactions;

namespace MovieFeedback.Service.Comments
{
    public class CommentService : ICommentService
    {
        private readonly MovieFeedbackDbContext _context;

        public CommentService(MovieFeedbackDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(int movieId, int userId, string content)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var comment = new Comment
                {
                    MovieId = movieId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EditCommentAsync(int commentId, int userId, string newContent)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);

            if (comment == null)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            comment.Content = newContent;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);

            if (comment == null)
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetMovieIdByCommentIdAsync(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
                throw new Exception("Comment not found.");

            return comment.MovieId;
        }


    }
}
