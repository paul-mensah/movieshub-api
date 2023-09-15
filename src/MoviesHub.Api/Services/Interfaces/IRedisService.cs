namespace MoviesHub.Api.Services.Interfaces;

public interface IRedisService
{
    Task<string> StringGetAsync(string key);
    Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiry);
}