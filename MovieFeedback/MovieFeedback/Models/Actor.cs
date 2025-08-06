using System;
using System.Collections.Generic;

namespace MovieFeedback.Models;

public partial class Actor
{
    public int ActorId { get; set; }

    public string Name { get; set; } = null!;
    public List<MovieActor> MovieActors { get; set; } = new();
    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
