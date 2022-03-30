using MoviesHub.Api.Models.Response;

namespace MoviesHub.Api.Services.Interfaces
{
    public interface IMoviesHttpService
    {
        Task<BaseResponse<string>> GetAsync(string url);
    }
}
