using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public class SpokenLanguage
{
    [JsonProperty("english_name")]
    public string EnglishName { get; set; }
    public string Name { get; set; }
    [JsonProperty("iso_639_1")]
    public string IsoName { get; set; }
}