using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;

namespace MoviesHub.Api.Services.Interfaces;

public interface IUserService
{
    Task<BaseResponse<UserResponse>> CreateUserAccount(CreateUserRequest request);
    Task<BaseResponse<UserResponse>> GetUserAccount(string mobileNumber);
    Task<BaseResponse<FavoriteMovieResponse>> AddFavoriteMovie(string mobileNumber, FavoriteMovieRequest request);
    Task<BaseResponse<FavoriteMovieResponse>> DeleteFavoriteMovie(string mobileNumber, string id);
    Task<BaseResponse<IEnumerable<FavoriteMovieResponse>>> GetFavoriteMovies(string mobileNumber);
}