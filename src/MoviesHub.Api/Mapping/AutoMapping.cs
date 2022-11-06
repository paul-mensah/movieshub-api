using AutoMapper;
using MoviesHub.Api.Models.Request;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Storage.Entities;

namespace MoviesHub.Api.Mapping;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        CreateMap<CreateUserRequest, User>().ReverseMap();
        CreateMap<User, UserResponse>().ReverseMap();
        CreateMap<FavoriteMovieRequest, FavoriteMovie>().ReverseMap();
        CreateMap<FavoriteMovie, FavoriteMovieResponse>().ReverseMap();
        CreateMap<OtpCode, OtpCodeResponse>().ReverseMap();
    }
}