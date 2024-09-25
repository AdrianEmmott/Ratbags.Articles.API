using MassTransit;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Ratbags.Shared.DTOs.Events.Events.CommentsRequest;
using System.ComponentModel.DataAnnotations;

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

    public async Task<Guid> CreateArticleAsync(CreateArticleDTO createArticleDTO)
    {
        if (createArticleDTO == null)
        {
            throw new ArgumentNullException(nameof(createArticleDTO));
        }

        var validationContext = new ValidationContext(createArticleDTO);
        Validator.ValidateObject(createArticleDTO, validationContext, validateAllProperties: true);

        var newArticle = new Article
        {
            Id = Guid.NewGuid(),
            Title = createArticleDTO.Title,
            Content = createArticleDTO.Content,
            Created = createArticleDTO.Created
        };

        var articleId = await _repository.CreateArticleAsync(newArticle);

        return articleId;
    }

    public async Task DeleteArticleAsync(Guid id)
    {
        await _repository.DeleteArticleAsync(id);
    }

    public async Task<IEnumerable<ArticleDTO>> GetAllArticlesAsync()
    {
        try
        {
            var articles = await _repository.GetAllArticlesAsync();

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

            //foreach (var article in articlesDTO)
            //{
            //    var response = await _massTrasitClient.GetResponse<CommentsForArticleResponse>(new CommentsForArticleRequest { ArticleId = article.Id.Value });

            //    article.Comments = response.Message.Comments;
            //}

            return articlesDTO.OrderBy(x => x.Created);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<ArticleDTO> GetArticleByIdAsync(Guid id)
    {
        try
        {
            var article = await _repository.GetArticleByIdAsync(id);

            var response = await _massTrasitClient.GetResponse<CommentsForArticleResponse>(new CommentsForArticleRequest { ArticleId = id });

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
        catch (Exception e)
        {
            _logger.LogError(e.Message); 
            throw;
        }
    }

    public async Task UpdateArticleAsync(ArticleDTO articleDTO)
    {
        if (articleDTO == null)
        {
            throw new ArgumentNullException(nameof(articleDTO));
        }

        var validationContext = new ValidationContext(articleDTO);
        Validator.ValidateObject(articleDTO, validationContext, validateAllProperties: true);

        var existingArticle = await _repository.GetArticleByIdAsync(articleDTO.Id.Value);

        if (existingArticle == null)
        {
            throw new Exception("Article not found");
        }

        existingArticle.Title = articleDTO.Title;
        existingArticle.Content = articleDTO.Content;
        existingArticle.Updated = DateTime.Now;

        await _repository.UpdateArticleAsync(existingArticle);
    }
}