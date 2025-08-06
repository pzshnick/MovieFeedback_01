using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int MovieId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public int? ParentCommentId { get; set; }
    public Comment ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
