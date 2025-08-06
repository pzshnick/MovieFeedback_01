using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public partial class Movie
{
    public int MovieId { get; set; }
    public int TmdbId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public double? Rating { get; set; }

    public string? Language { get; set; }

    public string? PosterPath { get; set; }
    public int? Runtime { get; set; }       
    public double? Popularity { get; set; }

    public DateTime? CreatedAt { get; set; }
    public List<MovieActor> MovieActors { get; set; } = new();
    public List<MovieGenre> MovieGenres { get; set; } = new();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Actor> Actors { get; set; } = new List<Actor>();

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
}
