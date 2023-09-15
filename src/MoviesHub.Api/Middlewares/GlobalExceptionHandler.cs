using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using MoviesHub.Api.Helpers;
using Newtonsoft.Json;

namespace MoviesHub.Api.Middlewares;

public static class GlobalExceptionHandler
{
    public static void ConfigureGlobalHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                IExceptionHandlerFeature exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature is not null)
                {
                    var baseException = exceptionHandlerFeature.Error;
                    logger.LogError(baseException, "Something went wrong:{exception}", baseException.Message);

                    var response = new
                    {
                        code = (int) HttpStatusCode.InternalServerError,
                        message = CommonResponses.InternalServerErrorResponseMessage
                    };

                    context.Response.ContentLength = JsonConvert.SerializeObject(response).Length;
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
        });
    }
}