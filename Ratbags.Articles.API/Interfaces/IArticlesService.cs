using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.API;
using Ratbags.Articles.API.Models.DTOs;
using Ratbags.Core.Models;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesService
{
    Task<Guid> CreateAsync(ArticleCreate model);
    Task<bool> DeleteAsync(Guid id);
    Task<PagedResult<ArticleListDTO>> GetAsync(GetArticlesParameters model);
    Task<ArticleDTO?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(ArticleUpdate model);
}
