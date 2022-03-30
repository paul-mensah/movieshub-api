using AutoMapper;
using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Storage.Models;

namespace MoviesHub.Api.Mapping;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        CreateMap<CreateUserRequest, User>().ReverseMap();
        CreateMap<User, UserResponse>().ReverseMap();
        CreateMap<FavoriteMovieRequest, FavoriteMovieResponse>().ReverseMap();
    }
}