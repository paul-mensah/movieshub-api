using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesHub.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Create a user account
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Create new user account", OperationId = nameof(CreateUserAccount))]
    public async Task<IActionResult> CreateUserAccount([FromBody] CreateUserRequest request)
    {
        var response = await _userService.CreateUserAccount(request);
        return !201.Equals(response.Code)
            ? StatusCode(response.Code, response)
            : CreatedAtRoute(nameof(GetUserAccount), new { response.Data.MobileNumber},  response);
    }
    
    /// <summary>
    /// Get user account with mobile number
    /// </summary>
    /// <param name="mobileNumber"></param>
    /// <returns></returns>
    [HttpGet("{mobileNumber}", Name = nameof(GetUserAccount))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Get user account", OperationId = nameof(GetUserAccount))]
    public async Task<IActionResult> GetUserAccount([FromRoute] string mobileNumber)
    {
        var response = await _userService.GetUserAccount(mobileNumber);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get user profile with user bearer token
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("profile")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Get user profile", OperationId = nameof(GetUserProfile))]
    public async Task<IActionResult> GetUserProfile()
    {
        var mobileNumber = User.GetUserData().MobileNumber;
        var response = await _userService.GetUserAccount(mobileNumber);
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Add movie to user's favorite movie list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("movies/favorites")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<FavoriteMovieResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Add movie to user's favorite list", OperationId = nameof(AddFavoriteMovieForUser))]
    public async Task<IActionResult> AddFavoriteMovieForUser([FromBody] FavoriteMovieRequest request)
    {
        var mobileNumber = User.GetUserData().MobileNumber;
        var response = await _userService.AddFavoriteMovie(mobileNumber, request);

        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Remove movie from user's favorite movie list
    /// </summary>
    /// <param name="movieId"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("movies/favorites/{movieId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<FavoriteMovieResponse>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Remove movie from user's favorite list", OperationId = nameof(DeleteFavoriteMovieForUser))]
    public async Task<IActionResult> DeleteFavoriteMovieForUser([FromRoute] string movieId)
    {
        var mobileNumber = User.GetUserData().MobileNumber;
        var response = await _userService.DeleteFavoriteMovie(mobileNumber, movieId);

        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Get user's favorite movies
    /// </summary>
    /// <param name="movieId"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("movies/favorites")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<IEnumerable<FavoriteMovieResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Get user's favorite movies", OperationId = nameof(GetFavoriteMoviesForUser))]
    public async Task<IActionResult> GetFavoriteMoviesForUser()
    {
        var mobileNumber = User.GetUserData().MobileNumber;
        var response = await _userService.GetFavoriteMovies(mobileNumber);

        return StatusCode(response.Code, response);
    }
}