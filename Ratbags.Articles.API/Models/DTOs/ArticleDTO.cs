using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Models.DTOs
{
    public class ArticleDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Introduction { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? Published { get; set; }

        public List<CommentDTO>? Comments { get; set; } = new List<CommentDTO>();

        public string AuthorName { get; set; } = string.Empty;

        public int Views { get; set; }
    }
}
