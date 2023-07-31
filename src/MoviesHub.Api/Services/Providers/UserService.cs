using Mapster;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Storage.Entities;
using Newtonsoft.Json;

namespace MoviesHub.Api.Services.Providers;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserCacheRepository _userCacheRepository;
    private readonly IFavoriteMovieRepository _favoriteMovieRepository;

    public UserService(ILogger<UserService> logger,
        IUserRepository userRepository,
        IUserCacheRepository userCacheRepository,
        IFavoriteMovieRepository favoriteMovieRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
        _userCacheRepository = userCacheRepository;
        _favoriteMovieRepository = favoriteMovieRepository;
    }
    
    public async Task<BaseResponse<UserResponse>> CreateUserAccount(CreateUserRequest request)
    {
        try
        {
            /*
             * Get user record from cache and query from DB if not found in cache 
             */
            var user = await _userCacheRepository.GetUserByMobileNumberAsync(request.MobileNumber) ??
                          await _userRepository.GetUserByMobileNumberAsync(request.MobileNumber);

            if (user is not null)
            {
                return CommonResponses.ErrorResponse
                    .ConflictResponse<UserResponse>("User account already created");
            }

            var newUserAccount = request.Adapt<User>();

            /*
             * Create in DB and cache if successful
             */
            bool isSavedInDb = await _userRepository.CreateAsync(newUserAccount);

            if (!isSavedInDb)
            {
                _logger.LogError("{mobileNumber}: An error occured creating user account in DB\nRequest => {request}",
                    request.MobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

                return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<UserResponse>();
            }

            bool userAccountIsCached = await _userCacheRepository.CacheUserAccount(newUserAccount);

            if (!userAccountIsCached)
            {
                _logger.LogError("{mobileNumber}: An error occured caching user account\nRequest => {request}",
                    request.MobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));
            }

            var userResponse = newUserAccount.Adapt<UserResponse>();
            return CommonResponses.SuccessResponse.CreatedResponse(userResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured creating user account\nRequest: {request}",
                request.MobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<UserResponse>();
        }
    }

    public async Task<BaseResponse<UserResponse>> GetUserAccount(string mobileNumber)
    {
        try
        {
            /*
             * Get user record from cache and query from DB if not found in cache 
             */
            var user = await _userCacheRepository.GetUserByMobileNumberAsync(mobileNumber) ??
                       await _userRepository.GetUserByMobileNumberAsync(mobileNumber);

            if (user is null)
            {
                return CommonResponses.ErrorResponse
                    .NotFoundResponse<UserResponse>("User not found");
            }

            var userResponse = user.Adapt<UserResponse>();
            return CommonResponses.SuccessResponse.OkResponse(userResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured getting user account", mobileNumber);
            
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<UserResponse>();
        }
    }

    public async Task<BaseResponse<FavoriteMovieResponse>> AddFavoriteMovie(string mobileNumber, FavoriteMovieRequest request)
    {
        try
        {
            var favoriteMovie = request.Adapt<FavoriteMovie>();
            favoriteMovie.UserMobileNumber = mobileNumber;

            bool isSaved = await _favoriteMovieRepository.AddAsync(favoriteMovie);

            return isSaved
                ? CommonResponses.SuccessResponse.CreatedResponse(favoriteMovie.Adapt<FavoriteMovieResponse>())
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<FavoriteMovieResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured adding movie to user favorite list.\nMovie => {newFavoriteMovie}",
                mobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FavoriteMovieResponse>();
        }
    }

    public async Task<BaseResponse<FavoriteMovieResponse>> DeleteFavoriteMovie(string mobileNumber, string id)
    {
        try
        {
            var favoriteMovie = await _favoriteMovieRepository.GetUserFavoriteMovieById(mobileNumber, id);

            if (favoriteMovie is null)
            {
                return CommonResponses.ErrorResponse.NotFoundResponse<FavoriteMovieResponse>("Movie not found");
            }

            bool isDeleted = await _favoriteMovieRepository.DeleteAsync(favoriteMovie);

            return isDeleted
                ? CommonResponses.SuccessResponse.DeleteResponse<FavoriteMovieResponse>("Movie removed successfully")
                : CommonResponses.ErrorResponse.FailedDependencyErrorResponse<FavoriteMovieResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured removing movie:{movieId} from user favorite list", 
                mobileNumber, id);

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FavoriteMovieResponse>();
        }
    }

    public async Task<BaseResponse<IEnumerable<FavoriteMovieResponse>>> GetFavoriteMovies(string mobileNumber)
    {
        try
        {
            var favoriteMoviesList = await _favoriteMovieRepository.GetFavoriteMovies(mobileNumber);

            var favoriteMoviesResponseEnumerable = favoriteMoviesList
                .Select(x => x.Adapt<FavoriteMovieResponse>())
                .AsEnumerable();

            return CommonResponses.SuccessResponse.OkResponse(favoriteMoviesResponseEnumerable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured getting favorite movies for user", 
                mobileNumber);

            return CommonResponses.ErrorResponse
                .InternalServerErrorResponse<IEnumerable<FavoriteMovieResponse>>();
        }
    }
}