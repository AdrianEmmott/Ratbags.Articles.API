using System.ComponentModel.DataAnnotations;

namespace Ratbags.Articles.API.Models.API
{
    public class ArticleUpdate
    {
        [Required(ErrorMessage = "Article id is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Introduction { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? Published { get; set; }

        public Guid AuthorUserId { get; set; }
    }
}
