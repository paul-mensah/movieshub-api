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
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;

namespace MoviesHub.Api.Services.Providers;

public class MoviesService : IMoviesService
{
    private readonly IMoviesHttpService _moviesHttpService;
    private readonly IFavoriteMovieRepository _favoriteMovieRepository;
    private readonly TheMovieDbConfig _theMovieDbConfig;

    public MoviesService(IMoviesHttpService moviesHttpService,
        IOptions<TheMovieDbConfig> theMovieDbConfig,
        IFavoriteMovieRepository favoriteMovieRepository)
    {
        _moviesHttpService = moviesHttpService;
        _favoriteMovieRepository = favoriteMovieRepository;
        _theMovieDbConfig = theMovieDbConfig.Value;
    }

    public async Task<BaseResponse<PaginatedMoviesListResponse>> GetMoviesList(string path, MoviesFilter filter)
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
                .OkResponse(JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(getMoviesResponse.Data));
    }

    public async Task<BaseResponse<FullMovieResponse>> GetMovieDetails(string id, string mobileNumber)
    {
        string apiKey = _theMovieDbConfig.ApiKey;
        string baseUrl = _theMovieDbConfig.BaseUrl;

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

        var fullMovieResponseList = await Task.WhenAll(tasksList);

        var movieResponse = fullMovieResponseList.FirstOrDefault();

        if (movieResponse is not null && !movieResponse.IsSuccessful)
        {
            return CommonResponses.ErrorResponse.NotFoundResponse<FullMovieResponse>("Movie not found");
        }

        var movieDetails = JsonConvert.DeserializeObject<MovieResponse>(movieResponse?.Data!);

        if (movieDetails is null)
        {
            return CommonResponses.ErrorResponse.NotFoundResponse<FullMovieResponse>("Movie not found");
        }
        
        var similarMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponseList[1].Data!);
        var recommendedMoviesList = JsonConvert.DeserializeObject<PaginatedMoviesListResponse>(fullMovieResponseList[2].Data!);
        var credit = JsonConvert.DeserializeObject<MovieCredit>(fullMovieResponseList[3].Data);
        var reviews = JsonConvert.DeserializeObject<MovieReviews>(fullMovieResponseList[4].Data);
        var videos = JsonConvert.DeserializeObject<MovieVideos>(fullMovieResponseList[5].Data);

        bool isFavoriteMovie = false;

        if (!string.IsNullOrEmpty(mobileNumber))
        {
            isFavoriteMovie = await _favoriteMovieRepository.IsUserFavoriteMovie(mobileNumber, movieDetails.Id);
        }

        var fullMovieResponse = new FullMovieResponse
        {
            Movie = movieDetails,
            Credits = credit,
            Reviews = reviews,
            Videos = videos,
            IsFavoriteMovie = isFavoriteMovie
        };

        if (similarMoviesList is not null &&  similarMoviesList.Results.Any())
        {
            fullMovieResponse.SimilarMovies = similarMoviesList.Results.GetRandomMovies(10);
        }
        
        if (recommendedMoviesList is not null &&  recommendedMoviesList.Results.Any())
        {
            fullMovieResponse.RecommendedMovies = recommendedMoviesList.Results.GetRandomMovies(10);
        }
        
        return CommonResponses.SuccessResponse.OkResponse(fullMovieResponse);
    }
}