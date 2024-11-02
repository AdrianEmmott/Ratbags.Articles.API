using System.ComponentModel.DataAnnotations;

namespace Ratbags.Articles.API.Models.API
{
    public class CarouselUpdate
    {
        [Required(ErrorMessage = "Carousel Id is required")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Article Id is required")]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = default!;
    }
}
