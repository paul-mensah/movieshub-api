namespace MoviesHub.Api.Models.Request;

public class FavoriteMovieRequest
{
    public int Id { get; set; }
    public string Title { get; set; }
    public double TotalVoteCount { get; set; }
    public string ImageUrl { get; set; }
    public double AverageRating { get; set; }
}