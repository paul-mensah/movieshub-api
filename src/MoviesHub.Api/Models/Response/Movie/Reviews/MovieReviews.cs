using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie.Reviews;

public sealed class MovieReviews
{
    public int Id { get; set; }
    public int Page { get; set; }
    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
    [JsonProperty("total_results")]
    public int TotalResults { get; set; }
    [JsonProperty("results")]
    public List<Review> Reviews { get; set; } = new List<Review>();
}