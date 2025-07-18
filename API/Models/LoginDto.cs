using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class LoginDto
{
    [Required]
    [StringLength(255)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
} 