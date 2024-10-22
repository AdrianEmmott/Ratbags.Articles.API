using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models.DB;

[Table("Articles")]
public partial class Article
{
    [Key]
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set;} // tag line for lists

    public string? Introduction { get; set; } // intro on the article itself

    public string Content { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public DateTime? Published { get; set; }

    public string? BannerImageUrl { get; set; }

    public Guid UserId { get; set; }
}
