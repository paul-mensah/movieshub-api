using MoviesHub.Api.Models.Response.Movie;

namespace MoviesHub.Api.Services.Interfaces
{
    public interface IMoviesHttpService
    {
        Task<MoviesHttpResponse> GetAsync(string url);
    }
}
