using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models.DB;

[Table("ArticleCarousels")]
[PrimaryKey("Id")]
public partial class ArticleCarousels
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("Article")]
    public Guid ArticleId { get; set; } 

    public string Title { get; set; } = default!;

    public virtual Article Article { get; set; } = default!;
}
