using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Models.Articles;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesService
{
    Task<Guid> CreateAsync(CreateArticleModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ArticleDTO>> GetAsync();
    Task<ArticleDTO?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(UpdateArticleModel model);
}
