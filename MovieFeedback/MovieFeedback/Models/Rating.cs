using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int MovieId { get; set; }

    public int UserId { get; set; }

    public int? Rating1 { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
