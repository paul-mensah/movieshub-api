using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie.Reviews;

public class Author
{
    public string Name { get; set; }
    public string Username { get; set; }
    [JsonProperty("avatar_path")]
    public string ImagePath { get; set; }
    public double? Rating { get; set; } = 0;
}