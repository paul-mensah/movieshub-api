namespace MoviesHub.Api.Models.Response.HubtelSms;

public sealed class SendSmsRequest
{
    public int Code { get; set; }
    public string Prefix { get; set; }
}