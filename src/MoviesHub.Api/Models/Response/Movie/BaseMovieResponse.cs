using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public class BaseMovieResponse
{
    public int Id { get; set; }
    public bool Adult { get; set; } = false;
    [JsonProperty("backdrop_path")] public string BackdropPath { get; set; }
    [JsonProperty("original_language")] public string OriginalLanguage { get; set; }
    [JsonProperty("original_title")] public string OriginalTitle { get; set; }
    public string Overview { get; set; }
    public double Popularity { get; set; }
    [JsonProperty("poster_path")] public string PosterPath { get; set; }
    [JsonProperty("release_date")] public string ReleaseDate { get; set; }
    [JsonProperty("vote_average")] public double VoteAverage { get; set; }
    [JsonProperty("vote_count")] public double VoteCount { get; set; }
    public bool Video { get; set; } = false;
    public string Title { get; set; }
}