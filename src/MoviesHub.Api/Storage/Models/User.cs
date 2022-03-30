namespace MoviesHub.Api.Storage.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MobileNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}