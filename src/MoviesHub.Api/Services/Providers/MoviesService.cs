using Flurl;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Filters;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Models.Response.Movie.Credits;
using MoviesHub.Api.Models.Response.Movie.Reviews;
using MoviesHub.Api.Models.Response.Movie.Videos;
using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MoviesHub.Api.Services.Providers;

public class MoviesService : IMoviesService
{
    private readonly ILogger<MoviesService> _logger;
    private readonly IMoviesHttpService _moviesHttpService;
    private readonly IConnectionMultiplexer _redis;
    private readonly TheMovieDbConfig _theMovieDbConfig;

    public MoviesService(ILogger<MoviesService> logger,
        IMoviesHttpService moviesHttpService,
        IOptions<TheMovieDbConfig> theMovieDbConfig,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _moviesHttpService = moviesHttpService;
        _redis = redis;
        _theMovieDbConfig = theMovieDbConfig.Value;
    }

    public async Task<BaseResponse<PaginatedMoviesListResponse>> GetMoviesList(string path, MoviesFilter filter)
    {
        try
        {
            var url = new Url(_theMovieDbConfig.BaseUrl).AppendPathSegment(path);
            url.SetQueryParams(new 
            {
                api_key = _theMovieDbConfig.ApiKey,
                filter.Language,
                filter.Page
            });
            
            var getMoviesResponse = await _moviesHttpService.GetAsync(url);

            return !getMoviesResponse.IsSuccessful
                ? CommonResponses.ErrorResponse.FailedDependencyErrorResponse<PaginatedMoviesListResponse>()
                : CommonResponses.SuccessResponse
                    .OkResponse<PaginatedMoviesListResponse>(
                        JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(getMoviesResponse.Data));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured getting movies\nPath => {path}\nFilter => {filter}", 
                path, JsonConvert.SerializeObject(filter, Formatting.Indented));
            
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<PaginatedMoviesListResponse>();
        }
    }

    public async Task<BaseResponse<FullMovieResponse>> GetMovieDetails(string id, string mobileNumber)
    {
        try
        {
            var apiKey = _theMovieDbConfig.ApiKey;
            var baseUrl = _theMovieDbConfig.BaseUrl;

            var movieDetailsTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}?api_key={apiKey}");
            var creditTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}/credits?api_key={apiKey}");
            var reviewsTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}/reviews?api_key={apiKey}");
            var videosTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}/videos?api_key={apiKey}");
            var similarMoviesListTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}/similar?api_key={apiKey}");
            var recommendedMoviesListTask = _moviesHttpService.GetAsync($"{baseUrl}/movie/{id}/recommendations?api_key={apiKey}");

            var tasksList = new[]
            {
                movieDetailsTask,
                similarMoviesListTask,
                recommendedMoviesListTask,
                creditTask,
                reviewsTask,
                videosTask
            };

            var fullMovieResponse = await Task.WhenAll(tasksList);

            var movieResponse = fullMovieResponse.FirstOrDefault();

            if (movieResponse is not null && !movieResponse.IsSuccessful)
            {
                return CommonResponses.ErrorResponse.NotFoundResponse<FullMovieResponse>("Movie not found");
            }

            var movieDetails = JsonConvert.DeserializeObject<MovieResponse>(movieResponse?.Data!);
            var similarMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponse[1].Data!);
            var recommendedMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponse[2].Data!);
            var credit = JsonConvert.DeserializeObject<MovieCredit>(fullMovieResponse[3].Data!);
            var reviews = JsonConvert.DeserializeObject<MovieReviews>(fullMovieResponse[4].Data!);
            var videos = JsonConvert.DeserializeObject<MovieVideos>(fullMovieResponse[5].Data!);

            var isFavoriteMovie = false;

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                var key = CommonConstants.User.GetFavoriteMovieKey(mobileNumber);
                isFavoriteMovie = await _redis.GetDatabase().HashExistsAsync(key, movieDetails?.Id);
            }
            
            return CommonResponses.SuccessResponse.OkResponse(new FullMovieResponse
            {
                Movie = movieDetails,
                SimilarMovies = similarMoviesList.Results.GetRandomMovies(10),
                RecommendedMovies = recommendedMoviesList.Results.GetRandomMovies(10),
                Credits = credit,
                Reviews = reviews,
                Videos = videos,
                IsFavoriteMovie = isFavoriteMovie
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[movie:{id}] An error occured getting full movie details", id);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FullMovieResponse>();
        }
    }
}