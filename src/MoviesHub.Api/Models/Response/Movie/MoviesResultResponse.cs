using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public class MoviesResultResponse : BaseMovieResponse
{
    [JsonProperty("genre_ids")] public List<int> GenreIds { get; set; } = new();
}