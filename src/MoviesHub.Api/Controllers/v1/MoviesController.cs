using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Filters;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesHub.Api.Controllers.v1;

[ApiController]
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
    /// Get top rated movies
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("top-rated")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedMoviesListResponse>))]
    [SwaggerOperation("Get top rated movies", OperationId = nameof(GetTopRatedMovies))]
    public async Task<IActionResult> GetTopRatedMovies([FromQuery] MoviesFilter filter)
    {
        var response = await _moviesService.GetMoviesList(CommonConstants.TopRatedMoviesPath, filter);

        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get popular movies
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("popular")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedMoviesListResponse>))]
    [SwaggerOperation("Get popular movies", OperationId = nameof(GetPopularMovies))]
    public async Task<IActionResult> GetPopularMovies([FromQuery] MoviesFilter filter)
    {
        var response = await _moviesService.GetMoviesList(CommonConstants.PopularMoviesPath, filter);

        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get upcoming movies
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("upcoming")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedMoviesListResponse>))]
    [SwaggerOperation("Get upcoming movies", OperationId = nameof(GetUpcomingMovies))]
    public async Task<IActionResult> GetUpcomingMovies([FromQuery] MoviesFilter filter)
    {
        var response = await _moviesService.GetMoviesList(CommonConstants.UpcomingMoviesPath, filter);

        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get trending movies
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpGet("trending")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedMoviesListResponse>))]
    [SwaggerOperation("Get trending movies", OperationId = nameof(GetTrendingMovies))]
    public async Task<IActionResult> GetTrendingMovies([FromQuery] MoviesFilter filter)
    {
        var response = await _moviesService.GetMoviesList(CommonConstants.TrendingMoviesPath, filter);

        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Get full details about a movie
    /// </summary>
    /// <param name="movieId"></param>
    /// <returns></returns>
    [HttpGet("{movieId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<FullMovieResponse>))]
    [SwaggerOperation("Get full details about a movie", OperationId = nameof(GetFullMovieDetails))]
    public async Task<IActionResult> GetFullMovieDetails([FromRoute] string movieId)
    {
        var response = await _moviesService.GetMovieDetails(movieId);
        return StatusCode(response.Code, response);
    }
}