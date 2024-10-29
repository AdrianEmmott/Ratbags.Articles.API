using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models.API;
using Ratbags.Articles.API.Models.DB;

namespace Ratbags.Articles.API.Repositories;

public class ArticlesViewsRepository : IArticleViewsRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArticlesViewsRepository> _logger;

    public ArticlesViewsRepository(ApplicationDbContext context, ILogger<ArticlesViewsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(ArticleViewsCreate model)
    {
        var articleViews = await _context.ArticleViews
            .FindAsync(model.ArticleId);

        if (articleViews == null)
        {
            articleViews = new ArticleViews();

            articleViews.ArticleId = model.ArticleId;
            articleViews.Views++;
            await _context.ArticleViews.AddAsync(articleViews);
        }
        else
        {
            articleViews.Views++;
            _context.ArticleViews.Update(articleViews);
        }

        try
        {
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError($"error updating viewcount for article {model.ArticleId}: {e.Message}");
            throw;
        }
    }

    public async Task<int> GetAsync(ArticleViewsGet model)
    {
        var viewCount = await _context.ArticleViews
            .FindAsync(model.ArticleId);

        return viewCount?.Views ?? 0;
    }
}