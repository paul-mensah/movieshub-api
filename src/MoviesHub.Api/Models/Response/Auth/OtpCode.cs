namespace MoviesHub.Api.Models.Response.Auth;
public class OtpCode
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");
    public string Prefix { get; set; }
    public int Code { get; set; }

    public OtpCode(string prefix, int code)
    {
        Prefix = prefix;
        Code = code;
    }
}