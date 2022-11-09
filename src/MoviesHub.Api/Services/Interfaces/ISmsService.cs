using MoviesHub.Api.Models.Response.HubtelSms;

namespace MoviesHub.Api.Services.Interfaces;

public interface ISmsService
{
    Task<bool> SendSms(string mobileNumber, SendSmsRequest request);
}