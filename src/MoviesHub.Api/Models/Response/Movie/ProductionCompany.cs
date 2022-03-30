using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public class ProductionCompany
{
    public int Id { get; set; }
    [JsonProperty("logo_path")]
    public string LogoPath { get; set; }
    public string Name { get; set; }
    [JsonProperty("origin_country")]
    public string OriginCountry { get; set; }
}