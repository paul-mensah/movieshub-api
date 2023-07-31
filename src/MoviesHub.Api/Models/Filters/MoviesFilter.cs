namespace MoviesHub.Api.Models.Filters;

public sealed class MoviesFilter
{
    public int Page { get; set; } = 1;
    public string Language { get; set; } = "en-US";
}