namespace MoviesHub.Api.Configurations;

public class BearerTokenConfig
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryDays { get; set; }
}