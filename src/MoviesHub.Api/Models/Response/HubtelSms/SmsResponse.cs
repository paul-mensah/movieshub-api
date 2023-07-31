namespace MoviesHub.Api.Models.Response.HubtelSms;

public sealed class SmsResponse
{
    public double Rate { get; set; }
    public string MessageId { get; set; }
    public string Status { get; set; }
    public string NetworkId { get; set; }
}