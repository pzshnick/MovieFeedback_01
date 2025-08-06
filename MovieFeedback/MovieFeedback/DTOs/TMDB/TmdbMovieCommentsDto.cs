using Newtonsoft.Json;

namespace MovieFeedback.DTOs.TMDB
{
    public class TmdbMovieCommentsDto
    {
        [JsonProperty("results")]
        public List<TmdbCommentDto> Results { get; set; }
    }
}
