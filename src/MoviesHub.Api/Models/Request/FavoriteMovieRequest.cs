using System.ComponentModel.DataAnnotations;

namespace MoviesHub.Api.Models.Request;

public class FavoriteMovieRequest
{
    [Required]
    public int MovieId { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; }
    [Required(AllowEmptyStrings = false)]
    public double TotalVoteCount { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string ImageUrl { get; set; }
    [Required]
    public double AverageRating { get; set; }
}