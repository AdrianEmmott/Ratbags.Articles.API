using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models.DB;

[Table("ArticleViews")]
public partial class ArticleViews
{
    [Key]
    [ForeignKey("Article")]
    public Guid ArticleId { get; set; } 

    public int Views { get; set; }

    public virtual Article Article { get; set; } = default!;
}
