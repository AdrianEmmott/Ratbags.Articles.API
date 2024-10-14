using Ratbags.Articles.API.Models.DB;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesRepository
{
    Task<Guid> CreateAsync(Article article);
    Task DeleteAsync(Guid id);
    //Task<IEnumerable<Article>> GetAsync();
    IQueryable<Article> GetQueryable();
    Task<Article?> GetByIdAsync(Guid id);
    Task UpdateAsync(Article article);
}
