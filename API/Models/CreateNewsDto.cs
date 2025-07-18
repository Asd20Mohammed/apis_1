using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class CreateNewsDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(24)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new List<string>();

    public bool IsPublished { get; set; } = true;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Url]
    public string? ImageUrl { get; set; }

    public DateTime? PublishedDate { get; set; }
} 