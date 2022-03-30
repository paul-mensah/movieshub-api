using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public class PaginatedMoviesListResponse
{
    public int Page { get; set; }
    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }
    [JsonProperty("total_results")]
    public int TotalResults { get; set; }
    public List<MoviesResultResponse> Results { get; set; } = new ();
}