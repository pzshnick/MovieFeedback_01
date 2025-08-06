using MovieFeedback.Models;
using System.Collections.Generic;

namespace MovieFeedback.ViewModels
{
    public class MainPageViewModel
    {
        public List<Movie> TopMovies { get; set; }
        public List<MovieFeedback.DTOs.TMDB.TmdbGenreDto> Genres { get; set; }
    }
}
