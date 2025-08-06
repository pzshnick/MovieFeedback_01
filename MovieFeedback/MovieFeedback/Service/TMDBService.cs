using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MovieFeedback.Models;
using Newtonsoft.Json;
using MovieFeedback.DTOs.TMDB;

namespace MovieFeedback.Service;

public class TMDBService
{
    // There was an API key
    private static readonly string ApiKey = "...";
    private static readonly HttpClient client = new HttpClient();

    // Getting basic information about the movie
    public static async Task<string> DiscoverMoviesAsync(int page = 1, string sortBy = "vote_average.desc", int minVotes = 1000, string minReleaseDate = "2010-01-01")
    {
        var url = $"https://api.themoviedb.org/3/discover/movie?api_key={ApiKey}" +
                  $"&language=en-US&sort_by={sortBy}&vote_count.gte={minVotes}&primary_release_date.gte={minReleaseDate}&page={page}";

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve data from TMDB API: {errorMessage}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Getting movie details
    public static async Task<TmdbMovieDetailsDto> GetMovieDetailsAsync(int movieId)
    {
        var response = await client.GetAsync($"https://api.themoviedb.org/3/movie/{movieId}?api_key={ApiKey}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var movieDetails = JsonConvert.DeserializeObject<TmdbMovieDetailsDto>(content);
            return movieDetails;
        }

        return null;
    }

    // Request for movie comments
    public static async Task<List<TmdbCommentDto>> GetMovieCommentsAsync(int tmdbId)
    {
        Console.WriteLine($"Requesting comments for TmdbId: {tmdbId}");

        var url = $"https://api.themoviedb.org/3/movie/{tmdbId}/reviews?api_key={ApiKey}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"No comments found for TmdbId: {tmdbId}");
                return new List<TmdbCommentDto>();
            }

            Console.WriteLine($"Error retrieving comments for TmdbId: {tmdbId}. Error: {errorMessage}");
            throw new Exception($"Failed to retrieve comments: {errorMessage}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var movieCommentsResponse = JsonConvert.DeserializeObject<TmdbMovieCommentsDto>(jsonResponse);

        Console.WriteLine($"Successfully retrieved {movieCommentsResponse?.Results.Count ?? 0} comments for TmdbId: {tmdbId}");
        return movieCommentsResponse?.Results ?? new List<TmdbCommentDto>();
    }

    // Request for movie actors
    public static async Task<List<Actor>> GetMovieActorsAsync(int tmdbId)
    {
        var url = $"https://api.themoviedb.org/3/movie/{tmdbId}/credits?api_key={ApiKey}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to retrieve actors for TmdbId: {tmdbId}. Error: {errorMessage}");
            return new List<Actor>();
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var creditsResponse = JsonConvert.DeserializeObject<TmdbMovieCreditsDto>(jsonResponse);

        return creditsResponse?.Cast?.Select(c => new Actor { Name = c.Name }).ToList() ?? new List<Actor>();
    }

    public static async Task<List<TMDBRecommendationDto>> GetRecommendedMoviesAsync(int movieTmdbId)
    {
        var url = $"https://api.themoviedb.org/3/movie/{movieTmdbId}/recommendations?api_key={ApiKey}&language=en-US&page=1";

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<TMDBRecommendationDto>();

        var content = await response.Content.ReadAsStringAsync();

        var result = System.Text.Json.JsonSerializer.Deserialize<TMDBRecommendationResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result?.Results ?? new List<TMDBRecommendationDto>();
    }
}