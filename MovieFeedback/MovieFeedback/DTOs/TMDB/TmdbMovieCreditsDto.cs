using Newtonsoft.Json;

namespace MovieFeedback.DTOs.TMDB
{
    public class TmdbMovieCreditsDto
    {
        [JsonProperty("cast")]
        public List<TmdbActorDto> Cast { get; set; }
    }
}