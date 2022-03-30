using System.ComponentModel.DataAnnotations;

namespace MoviesHub.Api.Models.Response.Auth;

public class VerifyOtpRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Prefix { get; set; }
    [Required]
    public int Code { get; set; }
}