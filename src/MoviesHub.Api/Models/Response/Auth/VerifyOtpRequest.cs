using System.ComponentModel.DataAnnotations;

namespace MoviesHub.Api.Models.Response.Auth;

public sealed class VerifyOtpRequest
{
    [Required(AllowEmptyStrings = false)]
    public string RequestId { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string Prefix { get; set; }
    [Required]
    public int Code { get; set; }
}