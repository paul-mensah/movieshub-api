﻿namespace MoviesHub.Api.Models.Response;

public class LoginResponse
{
    public string Token { get; set; }
    public int? Expiry { get; set; }
    public UserResponse User { get; set; }
}