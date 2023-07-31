using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public sealed class ProductionCountry
{
    [JsonProperty("iso_3166_1")]
    public string IsoName { get; set; }
    public string Name { get; set; }
}