namespace MoviesHub.Api.Storage.Entities;

public class FavoriteMovie
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int MovieId { get; set; }
    public string UserMobileNumber { get; set; }
    public string Title { get; set; }
    public double TotalVoteCount { get; set; }
    public string ImageUrl { get; set; }
    public double AverageRating { get; set; }
}