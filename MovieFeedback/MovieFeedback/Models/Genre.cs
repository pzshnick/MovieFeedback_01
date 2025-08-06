using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Name { get; set; } = null!;
    public List<MovieGenre> MovieGenres { get; set; } = new();

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
