namespace MoviesHub.Api.Configurations;

public class RedisConfig
{
    public string BaseUrl { get; set; }
    public int Database { get; set; }
    public int DataExpiryDays { get; set; }
}