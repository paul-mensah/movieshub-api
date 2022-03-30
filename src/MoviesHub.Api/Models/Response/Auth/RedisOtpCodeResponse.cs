namespace MoviesHub.Api.Models.Response.Auth;

public class RedisOtpCodeResponse
{
    public string Prefix { get; set; }
    public int Code { get; set; }
}