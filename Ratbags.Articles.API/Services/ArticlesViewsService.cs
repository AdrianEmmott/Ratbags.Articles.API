using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models.API;

namespace Ratbags.Articles.API.Services;

public class ArticlesViewsService : IArticleViewsService
{
    private readonly IArticleViewsRepository _repository;
    private readonly IMassTransitService _massTransitService;
    private readonly ILogger<ArticlesViewsService> _logger;

    public ArticlesViewsService(
        IArticleViewsRepository repository,
        IMassTransitService massTransitService,
        ILogger<ArticlesViewsService> logger)
    {
        _repository = repository;
        _massTransitService = massTransitService;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(ArticleViewsCreate model)
    {
        return await _repository.CreateAsync(model);
    }

    public async Task<int> GetAsync(ArticleViewsGet model)
    {
        var views = await _repository.GetAsync(model);

        return views;
    }
}