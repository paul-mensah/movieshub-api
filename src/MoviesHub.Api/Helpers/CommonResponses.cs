using System.Net;
using MoviesHub.Api.Models.Response;

namespace MoviesHub.Api.Helpers;

public static class CommonResponses
{
    private const string InternalServerErrorResponseMessage = "Something bad happened, try again later";
    private const string FailedDependencyErrorResponseMessage = "An error occured, try again later";
    private const string DefaultOkResponseMessage = "Retrieved successfully";
    private const string DefaultNotFoundResponseMessage = "Resource not found";
    private const string DefaultCreatedResponseMessage = "Created successfully";
    private const string DefaultUpdatedResponseMessage = "Updated successfully";
    private const string DefaultDeletedResponseMessage = "Deleted successfully";
    
    public static class ErrorResponse
    {
        public static BaseResponse<T> InternalServerErrorResponse<T>() =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.InternalServerError,
                Message = InternalServerErrorResponseMessage
            };
        
        public static BaseResponse<T> FailedDependencyErrorResponse<T>() =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.FailedDependency,
                Message = FailedDependencyErrorResponseMessage
            };
        
        public static BaseResponse<T> NotFoundResponse<T>(string? message) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.NotFound,
                Message = message ?? DefaultNotFoundResponseMessage
            };
        
        public static BaseResponse<T> BadRequestResponse<T>(string message) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.BadRequest,
                Message = message
            };
    }

    public static class SuccessResponse
    {
        public static BaseResponse<T> OkResponse<T>(T data, string? message = null) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.OK,
                Message = message ?? DefaultOkResponseMessage,
                Data = data
            };
        
        public static BaseResponse<T> CreatedResponse<T>(T data, string? message = null) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.Created,
                Message = message ?? DefaultCreatedResponseMessage,
                Data = data
            };
        
        public static BaseResponse<T> UpdatedResponse<T>(T data, string? message = null) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.OK,
                Message = message ?? DefaultUpdatedResponseMessage,
                Data = data
            };
        
        public static BaseResponse<T> DeleteResponse<T>(T data, string? message = null) =>
            new BaseResponse<T>
            {
                Code = (int) HttpStatusCode.OK,
                Message = message ?? DefaultDeletedResponseMessage,
                Data = data
            };
    }

}
