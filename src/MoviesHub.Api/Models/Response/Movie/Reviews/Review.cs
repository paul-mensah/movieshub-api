using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie.Reviews;

public class Review
{
    public string Id { get; set; }
    public string Author { get; set; }
    [JsonProperty("author_details")]
    public Author AuthorDetails { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }
}