namespace backend.Models;

public class AuthenticateResponse
{
    public ulong UserId { get; set; }
    public string? Login { get; set; }
    public string Token { get; set; }
}