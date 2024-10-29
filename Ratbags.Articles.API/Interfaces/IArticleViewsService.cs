using Ratbags.Articles.API.Models.API;

namespace Ratbags.Articles.API.Interfaces;

public interface IArticleViewsService
{
    Task<bool> CreateAsync(ArticleViewsCreate model);

    Task<int> GetAsync(ArticleViewsGet model);
}
