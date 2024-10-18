using Ratbags.Articles.API.Models;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Models;
using Ratbags.Core.Models.Articles;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticlesService
{
    Task<Guid> CreateAsync(CreateArticleModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<PagedResult<ArticleListDTO>> GetAsync(GetArticlesParameters model);
    Task<ArticleDTO?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(UpdateArticleModel model);
}
