using AutoMapper;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Storage.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MoviesHub.Api.Services.Providers;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly RedisConfig _redisConfig;
    private readonly IConnectionMultiplexer _redis;
    private readonly IMapper _mapper;

    public UserService(ILogger<UserService> logger,
        IOptions<RedisConfig> redisConfig,
        IConnectionMultiplexer redis,
        IMapper mapper)
    {
        _logger = logger;
        _redisConfig = redisConfig.Value;
        _redis = redis;
        _mapper = mapper;
    }
    
    public async Task<BaseResponse<UserResponse>> CreateUserAccount(CreateUserRequest request)
    {
        try
        {
            var userKey = CommonConstants.User.GetUserKey(request.MobileNumber);
            var userExists = _redis.GetDatabase().KeyExists(userKey);

            if (userExists)
                return CommonResponses.ErrorResponse
                    .ConflictResponse<UserResponse>("User account already created");
            
            var newUserAccount = _mapper.Map<User>(request);
            var serializedNewUserAccount = JsonConvert.SerializeObject(newUserAccount);
            
            var userCreatedResponse = await _redis.GetDatabase()
                .StringSetAsync(userKey, serializedNewUserAccount, TimeSpan.FromDays(_redisConfig.DataExpiryDays));

            if (!userCreatedResponse)
            {
                _logger.LogError("[{mobileNumber}] An error occured creating user account\nRequest => {request}",
                    request.MobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

                return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<UserResponse>();
            }

            var userResponse = _mapper.Map<UserResponse>(newUserAccount);
            return CommonResponses.SuccessResponse.CreatedResponse(userResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{mobileNumber}] An error occured creating user account\nRequest: {request}",
                request.MobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<UserResponse>();
        }
    }

    public async Task<BaseResponse<UserResponse>> GetUserAccount(string mobileNumber)
    {
        try
        {
            var userKey = CommonConstants.User.GetUserKey(mobileNumber);
            var user = await _redis.GetDatabase().StringGetAsync(userKey);

            if (!user.HasValue)
                return CommonResponses.ErrorResponse
                    .NotFoundResponse<UserResponse>("User not found");

            var userResponse = JsonConvert.DeserializeObject<UserResponse>(user);
            return CommonResponses.SuccessResponse.OkResponse<UserResponse>(userResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{mobileNumber}] An error occured getting user account", mobileNumber);
            
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<UserResponse>();
        }
    }

    public async Task<BaseResponse<FavoriteMovieResponse>> AddFavoriteMovie(string mobileNumber, FavoriteMovieRequest request)
    {
        try
        {
            var key = CommonConstants.User.GetFavoriteMovieKey(mobileNumber);
            
            await _redis.GetDatabase().HashSetAsync(key, new HashEntry[]
            {
                new (request.Id, JsonConvert.SerializeObject(request))
            });
            
            return CommonResponses.SuccessResponse.CreatedResponse(_mapper.Map<FavoriteMovieResponse>(request));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{mobileNumber}] An error occured adding movie to user favorite list.\nMovie => {newFavoriteMovie}",
                mobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FavoriteMovieResponse>();
        }
    }

    public async Task<BaseResponse<FavoriteMovieResponse>> DeleteFavoriteMovie(string mobileNumber, string movieId)
    {
        try
        {
            var key = CommonConstants.User.GetFavoriteMovieKey(mobileNumber);

            var isDeleted = await _redis.GetDatabase().HashDeleteAsync(key, movieId);

            return isDeleted
                ? CommonResponses.SuccessResponse
                    .DeleteResponse<FavoriteMovieResponse>("Movie removed successfully")
                : CommonResponses.ErrorResponse
                    .FailedDependencyErrorResponse<FavoriteMovieResponse>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{mobileNumber}] An error occured removing movie:{movieId} from user favorite list", 
                mobileNumber, movieId);

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FavoriteMovieResponse>();
        }
    }

    public async Task<BaseResponse<IEnumerable<FavoriteMovieResponse>>> GetFavoriteMovies(string mobileNumber)
    {
        try
        {
            var key = CommonConstants.User.GetFavoriteMovieKey(mobileNumber);
            var redisResponse = await _redis.GetDatabase().HashGetAllAsync(key);

            var favoriteMoviesList = redisResponse.Select(x =>
                    JsonConvert.DeserializeObject<FavoriteMovieResponse>(x.Value))
                .AsEnumerable();

            return CommonResponses.SuccessResponse.OkResponse(favoriteMoviesList);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{mobileNumber}] An error occured getting favorite movies for user", 
                mobileNumber);

            return CommonResponses.ErrorResponse
                .InternalServerErrorResponse<IEnumerable<FavoriteMovieResponse>>();
        }
    }
}