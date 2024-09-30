using Ratbags.Shared.DTOs.Events.DTOs.Articles;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesService
{
    Task<Guid> CreateArticleAsync(CreateArticleDTO article);
    Task<bool> DeleteArticleAsync(Guid id);
    Task<IEnumerable<ArticleDTO>> GetAllArticlesAsync();
    Task<ArticleDTO?> GetArticleByIdAsync(Guid id);
    Task<bool> UpdateArticleAsync(ArticleDTO article);
}
