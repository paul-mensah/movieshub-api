using MoviesHub.Api.Models.Response.Movie.Credits;
using MoviesHub.Api.Models.Response.Movie.Reviews;
using MoviesHub.Api.Models.Response.Movie.Videos;

namespace MoviesHub.Api.Models.Response.Movie;

public class FullMovieResponse
{
    public MovieResponse? Movie { get; set; }
    public List<MoviesResultResponse> SimilarMovies { get; set; } = new List<MoviesResultResponse>();
    public List<MoviesResultResponse> RecommendedMovies { get; set; } = new List<MoviesResultResponse>();
    public MovieCredit? Credits { get; set; }
    public MovieReviews? Reviews { get; set; }
    public MovieVideos? Videos { get; set; }
    public bool IsFavoriteMovie { get; set; }
}