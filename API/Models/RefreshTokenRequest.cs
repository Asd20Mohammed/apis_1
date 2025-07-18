using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
} 