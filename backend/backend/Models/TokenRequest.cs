using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class TokenRequest
{
    [Required]
    public string Token { get; set; }
}