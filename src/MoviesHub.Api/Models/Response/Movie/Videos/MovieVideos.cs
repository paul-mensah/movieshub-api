using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie.Videos;

public sealed class MovieVideos
{
    [JsonProperty("results")]
    public List<MovieVideo> Videos { get; set; } = new List<MovieVideo>();
}