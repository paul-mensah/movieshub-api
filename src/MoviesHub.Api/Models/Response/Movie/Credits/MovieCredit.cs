namespace MoviesHub.Api.Models.Response.Movie.Credits;

public class MovieCredit
{
    public List<MovieCast> Cast { get; set; } = new List<MovieCast>();
    public List<MovieCrew> Crew { get; set; } = new List<MovieCrew>();
}