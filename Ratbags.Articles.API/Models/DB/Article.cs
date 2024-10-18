using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models.DB;

[Table("Articles")]
public partial class Article
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set;} = null!;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public DateTime? Published { get; set; }

    public string? ImageUrl { get; set; }
}
