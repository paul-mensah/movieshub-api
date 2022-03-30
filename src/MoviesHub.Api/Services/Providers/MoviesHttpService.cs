using Flurl.Http;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace MoviesHub.Api.Services.Providers
{
  public class MoviesHttpService : IMoviesHttpService
  {
      private readonly ILogger _logger;
      
      public MoviesHttpService(ILogger logger)
      {
          _logger = logger;
      }

      public async Task<BaseResponse<string>> GetAsync(string url)
      {
          if (string.IsNullOrEmpty(url))
              return CommonResponses.ErrorResponse.BadRequestResponse<string>("Provide url");

          try
          {
              IFlurlResponse serverResponse;
              
              try
              {
                  serverResponse = await url.AllowAnyHttpStatus().GetAsync();
              }
              catch (Exception e)
              {
                  _logger.Error(e, "An error occured performing get api request\nUrl: {url}", url);
                  
                  return CommonResponses.ErrorResponse.InternalServerErrorResponse<string>();
              }

              var rawResponse = await serverResponse.GetStringAsync();

              return new BaseResponse<string>
              {
                  Code = serverResponse.StatusCode,
                  Data = rawResponse
              };
          }
          catch (Exception e)
          {
              _logger.Error(e, "An error occured getting record with\nUrl: {url}", url);

              return CommonResponses.ErrorResponse.InternalServerErrorResponse<string>();
          }
      }
  }
}
