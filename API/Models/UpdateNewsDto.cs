using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class UpdateNewsDto
{
    [StringLength(200, MinimumLength = 5)]
    public string? Title { get; set; }

    [StringLength(10000, MinimumLength = 10)]
    public string? Content { get; set; }

    [StringLength(100)]
    public string? Author { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public List<string>? Tags { get; set; }

    public bool? IsPublished { get; set; }

    [StringLength(500)]
    public string? Summary { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    public DateTime? PublishedDate { get; set; }
} 