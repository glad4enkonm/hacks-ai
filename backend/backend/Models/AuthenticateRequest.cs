using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class AuthenticateRequest
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string PasswordHash { get; set; }
}