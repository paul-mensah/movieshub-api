namespace MoviesHub.Api.Models.Response;

public sealed class GenerateTokenResponse
{
    public string BearerToken { get; set; }
    public int? Expiry { get; set; }
}