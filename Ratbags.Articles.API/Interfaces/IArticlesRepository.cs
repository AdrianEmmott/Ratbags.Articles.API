using Ratbags.Articles.API.Models;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesRepository
{
    Task<Guid> CreateArticleAsync(Article article);
    Task DeleteArticleAsync(Guid id);
    Task<IEnumerable<Article>> GetAllArticlesAsync();
    Task<Article> GetArticleByIdAsync(Guid id);
    Task UpdateArticleAsync(Article article);
}
