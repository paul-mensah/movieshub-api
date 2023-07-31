using Newtonsoft.Json;

namespace MoviesHub.Api.Models.Response.Movie;

public sealed class MovieResponse : BaseMovieResponse
{
    public decimal Budget { get; set; }
    public decimal Revenue { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public string HomePage { get; set; }
    [JsonProperty("imdb_id")]
    public string ImdbId { get; set; }
    public int Runtime { get; set; }
    public string Status { get; set; }
    public string TagLine { get; set; }
    [JsonProperty("spoken_languages")]
    public List<SpokenLanguage> SpokenLanguages { get; set; } = new();
    [JsonProperty("production_companies")]
    public List<ProductionCompany> ProductionCompanies { get; set; } = new();
    [JsonProperty("production_countries")]
    public List<ProductionCountry> ProductionCountries { get; set; } = new();
}