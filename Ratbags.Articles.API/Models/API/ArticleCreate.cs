using System.ComponentModel.DataAnnotations;

namespace Ratbags.Articles.API.Models.API
{
    public class ArticleCreate
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public string? Introduction { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = default!;

        public string? BannerImageUrl { get; set; }

        public DateTime Created { get; set; }

        public Guid AuthorUserId { get; set; }
    }
}
