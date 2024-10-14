using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.CommentsRequest;
using Ratbags.Core.Models.Articles;

namespace Ratbags.Articles.API.Services;

public class ArticlesService : IArticlesService
{
    private readonly IArticlesRepository _repository;
    private readonly IRequestClient<CommentsForArticleRequest> _massTrasitClient;
    private readonly ILogger<ArticlesService> _logger;

    public ArticlesService(IArticlesRepository repository,
        IRequestClient<CommentsForArticleRequest> massTrasitClient,
        ILogger<ArticlesService> logger)
    {
        _repository = repository;
        _massTrasitClient = massTrasitClient;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateArticleModel model)
    {
        var newArticle = new Article
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Content = model.Content,
            Created = model.Created
        };

        try
        {
            var articleId = await _repository.CreateAsync(newArticle);

            return articleId;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError($"Error inserting article: {e.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var article = await _repository.GetByIdAsync(id);

        if (article == null)
        {
            return false;
        }

        try
        {
            await _repository.DeleteAsync(id);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError($"Error deleting article {id}: {e.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ArticleDTO>> GetAsync()
    {
        _logger.LogInformation("getting articles...");
        var articles = await _repository.GetAsync();

        var articlesDTO = articles.Select(
            article => new ArticleDTO
            {
                Id = article.Id,
                Title = article.Title,
                Created = article.Created,
                Updated = article.Updated,
                Published = article.Published
            })
            .ToList();

        // TODO - write something in comments that gets the number of comments / article
        
        return articlesDTO.OrderByDescending(x => x.Created);
    }

    public async Task<ArticleDTO?> GetByIdAsync(Guid id)
    {
        var article = await _repository.GetByIdAsync(id);

        if (article != null)
        {
            try
            {
                var response = await _massTrasitClient
                                .GetResponse<CommentsForArticleResponse>
                                (new CommentsForArticleRequest
                                {
                                    ArticleId = id
                                });

                return new ArticleDTO
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Created = article.Created,
                    Updated = article.Updated,
                    Published = article.Published,
                    Comments = response.Message.Comments
                };
            }
            catch (MassTransitException e)
            {
                _logger.LogError($"Error sending comments request for article {id}: {e.Message}");
                throw;
            }
        }

        return null;
    }

    public async Task<bool> UpdateAsync(UpdateArticleModel model)
    {
        var existingArticle = await _repository.GetByIdAsync(model.Id);

        if (existingArticle == null)
        {
            return false;
        }

        existingArticle.Title = model.Title;
        existingArticle.Content = model.Content;
        existingArticle.Updated = DateTime.Now;

        try
        {
            await _repository.UpdateAsync(existingArticle);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError($"Error updating article {model.Id}: {e.Message}");
            throw;
        }
    }
}