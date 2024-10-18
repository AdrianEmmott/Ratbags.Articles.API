using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesRepository
{
    Task<Guid> CreateAsync(Article article);
    Task DeleteAsync(Guid id);
    Task<(List<Article> Articles, int TotalCount)> GetArticlesAsync(GetArticlesParameters model);
    Task<Article?> GetByIdAsync(Guid id);
    Task UpdateAsync(Article article);
}
