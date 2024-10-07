using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Interfaces;

public interface IService
{
    Task<Guid> CreateAsync(CreateArticleDTO article);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<ArticleDTO>> GetAsync();
    Task<ArticleDTO?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(ArticleDTO article);
}
