using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MovieFeedback.ViewModels
{
    public class MovieEditViewModel
    {
        public int MovieId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Genres { get; set; } = new List<string>();

        [BindNever]
        public List<string> AvailableGenres { get; set; } = new List<string>();
    }
}
