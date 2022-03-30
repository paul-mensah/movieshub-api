namespace MoviesHub.Api.Models.Filters;

public class MoviesFilter
{
    public int Page { get; set; } = 1;
    public string Language { get; set; } = "en-US";
}