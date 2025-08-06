using Newtonsoft.Json;

namespace MovieFeedback.DTOs.TMDB
{
    public class TmdbActorDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
