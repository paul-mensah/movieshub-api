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
using ILogger = Serilog.ILogger;

namespace MoviesHub.Api.Services.Providers;

public class MoviesService : IMoviesService
{
    private readonly ILogger _logger;
    private readonly IMoviesHttpService _moviesHttpService;
    private readonly TheMovieDbConfig _theMovieDbConfig;

    public MoviesService(ILogger logger,
        IMoviesHttpService moviesHttpService,
        IOptions<TheMovieDbConfig> theMovieDbConfig)
    {
        _logger = logger;
        _moviesHttpService = moviesHttpService;
        _theMovieDbConfig = theMovieDbConfig.Value;
    }
    
    public async Task<BaseResponse<PaginatedMoviesListResponse>> GetMoviesList(string path, MoviesFilter filter)
    {
        try
        {
            var moviesUrl =
                $"{_theMovieDbConfig.BaseUrl}/{path}?api_key={_theMovieDbConfig.ApiKey}&language={filter.Language}&page={filter.Page}";

            var getMoviesResponse = await _moviesHttpService.GetAsync(moviesUrl);

            if (!200.Equals(getMoviesResponse.Code))
            {
                return new BaseResponse<PaginatedMoviesListResponse>
                {
                    Code = getMoviesResponse.Code,
                    Message = "An error occured, try again later."
                };
            }

            var response = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(getMoviesResponse.Data);

            return CommonResponses.SuccessResponse.OkResponse<PaginatedMoviesListResponse>(response);
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occured getting movies from {path}", path);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<PaginatedMoviesListResponse>();
        }
    }

    public async Task<BaseResponse<FullMovieResponse>> GetMovieDetails(string id)
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
            
            var tasksList = new []
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

            if (!200.Equals(movieResponse?.Code))
            {
                return CommonResponses.ErrorResponse.NotFoundResponse<FullMovieResponse>("Movie not found");
            }

            var movieDetails = JsonConvert.DeserializeObject<MovieResponse>(movieResponse.Data);
            var similarMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponse[1].Data);
            var recommendedMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponse[2].Data);
            var credit = JsonConvert.DeserializeObject<MovieCredit>(fullMovieResponse[3].Data);
            var reviews = JsonConvert.DeserializeObject<MovieReviews>(fullMovieResponse[4].Data);
            var videos = JsonConvert.DeserializeObject<MovieVideos>(fullMovieResponse[5].Data);

            var response = new FullMovieResponse
            {
                Movie = movieDetails,
                SimilarMovies = similarMoviesList?.Results.GetRandomMovies(10),
                RecommendedMovies = recommendedMoviesList?.Results.GetRandomMovies(10),
                Credits = credit,
                Reviews = reviews,
                Videos = videos
            };
            
            return CommonResponses.SuccessResponse.OkResponse(response);
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occured getting full movie:{id} details", id);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FullMovieResponse>();
        }
    }
}