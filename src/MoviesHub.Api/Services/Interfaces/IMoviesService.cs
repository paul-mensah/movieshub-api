using MoviesHub.Api.Models.Filters;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;

namespace MoviesHub.Api.Services.Interfaces;

public interface IMoviesService
{
    Task<BaseResponse<PaginatedMoviesListResponse>> GetMoviesList(string path, MoviesFilter filter);
    Task<BaseResponse<FullMovieResponse>> GetMovieDetails(string id);
}