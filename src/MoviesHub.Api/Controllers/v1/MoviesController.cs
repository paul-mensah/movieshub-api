using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Filters;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesHub.Api.Controllers.v1;

[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/v1/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
public class MoviesController : ControllerBase
{
    private readonly IMoviesService _moviesService;

    public MoviesController(IMoviesService moviesService)
    {
        _moviesService = moviesService;
    }

    /// <summary>
    /// Get movies
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedMoviesListResponse>))]
    [SwaggerOperation("Get movies", OperationId = nameof(GetMovies))]
    public async Task<IActionResult> GetMovies([FromQuery] MoviesFilter filter)
    {
        string moviesPath = filter.Type.ToLower().Trim() switch
        {
            "popular" => CommonConstants.Movies.PopularMoviesPath,
            "top-rated" => CommonConstants.Movies.TopRatedMoviesPath,
            "trending" => CommonConstants.Movies.TrendingMoviesPath,
            "upcoming" => CommonConstants.Movies.UpcomingMoviesPath,
            _ => CommonConstants.Movies.TrendingMoviesPath
        };
        
        var response = await _moviesService.GetMoviesList(moviesPath, filter);
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Get full details about a movie
    /// </summary>
    /// <param name="movieId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{movieId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<FullMovieResponse>))]
    [SwaggerOperation("Get full details about a movie", OperationId = nameof(GetFullMovieDetails))]
    public async Task<IActionResult> GetFullMovieDetails([FromRoute] string movieId)
    {
        string mobileNumber = User.GetUserData().MobileNumber;
        var response = await _moviesService.GetMovieDetails(movieId, mobileNumber);
        return StatusCode(response.Code, response);
    }
}