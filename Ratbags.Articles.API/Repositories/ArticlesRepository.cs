using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;

namespace Ratbags.Articles.API.Repositories;

public class ArticlesRepository : IArticlesRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArticlesRepository> _logger;

    public ArticlesRepository(ApplicationDbContext context, ILogger<ArticlesRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(Article article)
    {
        await _context.Articles.AddAsync(article);
        await _context.SaveChangesAsync();

        return article.Id;
    }

    public async Task DeleteAsync(Guid id)
    {
        var model = new Article { Id = id };

        _context.Articles.Attach(model);
        _context.Articles.Remove(model);

        await _context.SaveChangesAsync();
    }

    public async Task<(List<Article> Articles, int TotalCount)> GetArticlesAsync(GetArticlesParameters model)
    {
        var query = _context.Articles;

        // count before building real query
        var totalCount = await query.CountAsync();

        // real query - set to IQueryable to stop skip and take blowing up -
        // orderBy changes it to IOrderedQueryable and orderby needs to come before anything else
        var articles = query.OrderByDescending(a => a.Created).AsQueryable();

        if (model.Skip > 0 || model.Take > 0)
        {
            articles = articles.Skip(model.Skip).Take(model.Take);
        }

        return (await articles.ToListAsync(), totalCount);
    }

    public async Task<Article?> GetByIdAsync(Guid id)
    {
        var result = await _context.Articles.FindAsync(id);
        return result;
    }

    public async Task UpdateAsync(Article article)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
    }
}