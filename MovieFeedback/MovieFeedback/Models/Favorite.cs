using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public class Favorite
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
    public Movie Movie { get; set; }
}
