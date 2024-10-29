using System.ComponentModel.DataAnnotations;

namespace Ratbags.Articles.API.Models.API
{
    public class CarouselCreate
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }=string.Empty;
    }
}
