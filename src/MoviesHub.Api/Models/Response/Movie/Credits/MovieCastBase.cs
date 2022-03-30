using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie.Credits;

public class MovieCastBase
{
    public int Id { get; set; }
    public string Name { get; set; }
    [JsonProperty("original_name")] public string OriginalName { get; set; }
    public double Popularity { get; set; }
    [JsonProperty("profile_path")] public string ProfilePath { get; set; }
    public int Gender { get; set; }
    public bool Adult { get; set; } = false;
    [JsonProperty("known_for_department")] public string KnownForDepartment { get; set; }
    [JsonProperty("cast_id")]
    public int CastId { get; set; } public string Character { get; set; }
    [JsonProperty("credit_id")] public string CreditId { get; set; }
}