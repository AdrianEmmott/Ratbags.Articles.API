using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;

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

    public async Task<Guid> CreateArticleAsync(Article article)
    {
        await _context.Articles.AddAsync(article);
        await _context.SaveChangesAsync();

        return article.Id;
    }

    public async Task DeleteArticleAsync(Guid id)
    {
        var article = await _context.Articles.FindAsync(id);

        if (article != null)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        _logger.LogInformation($"get all articles");
        return await _context.Articles.ToListAsync();
    }

    public async Task<Article?> GetArticleByIdAsync(Guid id)
    {
        _logger.LogInformation($"get article id {id}");

        var result = await _context.Articles.FirstOrDefaultAsync(x => x.Id == id);

        if (result != null)
        {
            return result;
        }

        return null;
    }

    public async Task UpdateArticleAsync(Article article)
    {
        _logger.LogInformation($"update article id {article.Id}");

        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
    }
}