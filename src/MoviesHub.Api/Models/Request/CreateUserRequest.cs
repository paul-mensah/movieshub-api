using System.ComponentModel.DataAnnotations;

namespace MoviesHub.Api.Models.Request;

public class CreateUserRequest
{
    [Required(AllowEmptyStrings = false)]
    public string FirstName { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string LastName { get; set; }
    [Required(AllowEmptyStrings = false)]
    [MinLength(10)]
    [MaxLength(10)]
    public string MobileNumber { get; set; }
}