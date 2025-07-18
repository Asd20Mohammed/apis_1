using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string? Username { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(20)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    [Url]
    public string? ProfileImageUrl { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public DateTime? DateOfBirth { get; set; }
} 