namespace MoviesHub.Api.Configurations;

public sealed class RedisConfig
{
    public string BaseUrl { get; set; }
    public int Database { get; set; }
    public string CertificatePath { get; set; }
    public int DataExpiryDays { get; set; }
    public int OtpCodeExpiryInMinutes { get; set; }
}