namespace MoviesHub.Api.Configurations;

public sealed class HubtelSmsConfig
{
    public string BaseUrl { get; set; }
    public string ClientKey { get; set; }
    public string ClientSecret { get; set; }
    public string SenderId { get; set; }
}