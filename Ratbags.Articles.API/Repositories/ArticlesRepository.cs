using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
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

    public async Task<IEnumerable<Article>> GetAsync()
    {
        _logger.LogInformation($"database cxn: {_context.Database.GetConnectionString()}");
        return await _context.Articles.ToListAsync();
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