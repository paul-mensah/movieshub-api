using Flurl.Http;
using MoviesHub.Api.Models.Response.Movie;
using MoviesHub.Api.Services.Interfaces;

namespace MoviesHub.Api.Services.Providers
{
  public class MoviesHttpService : IMoviesHttpService
  {
      private readonly ILogger<MoviesHttpService> _logger;
      
      public MoviesHttpService(ILogger<MoviesHttpService> logger)
      {
          _logger = logger;
      }

      public async Task<MoviesHttpResponse> GetAsync(string url)
      {
          try
          {
              if (string.IsNullOrEmpty(url))
                  return new MoviesHttpResponse(false, null);
              
              var apiResponse = await url.AllowAnyHttpStatus().GetAsync();
              var rawResponse = await apiResponse.ResponseMessage.Content.ReadAsStringAsync();
              
              _logger.LogInformation("Response from movies api\nUrl => {url}\nResponse => {movieApiResponse}", 
                  url, rawResponse);

              return new MoviesHttpResponse(
                  isSuccessful: apiResponse.ResponseMessage.IsSuccessStatusCode, 
                  data: rawResponse);
          }
          catch (Exception e)
          {
              _logger.LogError(e, "An error occured getting record with\nUrl => {url}", url);
              return new MoviesHttpResponse(false, null);
          }
      }
  }
}
