namespace MoviesHub.Api.Models.Response.Movie;

public sealed class FavoriteMovieResponse
{
    public string Id { get; set; }
    public int MovieId { get; set; }
    public string Title { get; set; }
    public double TotalVoteCount { get; set; }
    public string ImageUrl { get; set; }
    public double AverageRating { get; set; }
}