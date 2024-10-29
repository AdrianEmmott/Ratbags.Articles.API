using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ratbags.Articles.API.Models.DB;

[Table("ArticleCarouselImages")]
[PrimaryKey("Id")]
public partial class ArticleCarouselImages
{
    [Key]
    public Guid Id { get; set; }

    [Key]
    [ForeignKey("ArticleCarousels")]
    public Guid CarouselId { get; set; }

    public string ImageUrl { get; set; }

    public string Description { get; set; }

    public virtual ArticleCarousels Carousel { get; set; }
}
