using Ratbags.Articles.API.Models;

namespace Ratbags.Articles.API.Interfaces;

public interface IRepository
{
    Task<Guid> CreateAsync(Article article);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Article>> GetAsync();
    Task<Article?> GetByIdAsync(Guid id);
    Task UpdateAsync(Article article);
}
