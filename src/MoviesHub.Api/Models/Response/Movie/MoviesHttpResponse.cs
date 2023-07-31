namespace MoviesHub.Api.Models.Response.Movie;

public class MoviesHttpResponse
{
    public bool IsSuccessful { get; set; }
    public string Data { get; set; }

    public MoviesHttpResponse(bool isSuccessful, string data)
    {
        IsSuccessful = isSuccessful;
        Data = data;
    }
}