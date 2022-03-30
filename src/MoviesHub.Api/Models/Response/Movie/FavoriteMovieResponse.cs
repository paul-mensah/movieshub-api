namespace MoviesHub.Api.Models.Response.Movie;

public class FavoriteMovieResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public double TotalVoteCount { get; set; }
    public string ImageUrl { get; set; }
    public double AverageRating { get; set; }
}