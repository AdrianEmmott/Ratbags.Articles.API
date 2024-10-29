using System.ComponentModel.DataAnnotations;

namespace Ratbags.Articles.API.Models.API
{
    public class ArticleViewsGet
    {
        [Required(ErrorMessage = "Article Id is required")]
        public Guid ArticleId { get; set; }
    }
}
