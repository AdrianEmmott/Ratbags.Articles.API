using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models;

[Table("Articles")]
public partial class Article
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public DateTime? Published { get; set; }

    public string? ImageUrl { get; set; }
}
