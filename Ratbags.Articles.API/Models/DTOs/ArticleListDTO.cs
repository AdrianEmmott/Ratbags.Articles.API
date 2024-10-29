namespace Ratbags.Articles.API.Models.DTOs
{
    public class ArticleListDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;


        public string? ThumbnailImageUrl { get; set; }

        public string? Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Published { get; set; }

        public int CommentCount { get; set; }
    }
}
