namespace MoviesHub.Api.Models.Response.Auth;

public sealed class OtpCodeResponse
{
    public string RequestId { get; set; }
    public string Prefix { get; set; }
}