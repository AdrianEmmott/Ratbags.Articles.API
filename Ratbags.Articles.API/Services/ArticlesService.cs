using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.API;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Articles.API.Models.DTOs;
using Ratbags.Core.Models;

namespace Ratbags.Articles.API.Services;

public class ArticlesService : IArticlesService
{
    private readonly IArticlesRepository _repository;
    private readonly IArticlesServiceBusService _serviceBusService;
    private readonly ILogger<ArticlesService> _logger;

    public ArticlesService(
        IArticlesRepository repository,
        IArticlesServiceBusService serviceBusService,
        ILogger<ArticlesService> logger)
    {
        _repository = repository;
        _serviceBusService = serviceBusService;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(ArticleCreate model)
    {
        var newArticle = new Article
        {
            Id = Guid.NewGuid(),
            Title = model.Title,
            Content = model.Content,
            Description = model.Description,
            Introduction = model.Introduction,
            BannerImageUrl = model.BannerImageUrl,
            Created = model.Created,
            UserId = model.AuthorUserId,
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

    public async Task<PagedResult<ArticleListDTO>> GetAsync(GetArticlesParameters model)
    {
        _logger.LogInformation("getting articles...");

        var (articles, totalCount) = await _repository.GetArticlesAsync(model);

        var listDTOs = new List<ArticleListDTO>();

        foreach (var article in articles)
        {
            var dto = new ArticleListDTO
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                ThumbnailImageUrl = article.BannerImageUrl ?? string.Empty,
                Published = article.Published
            };

            listDTOs.Add(dto);
        }

        var articleCommentCounts =
                    await _serviceBusService
                        .GetArticlesCommentsCount(articles.Select(x => x.Id)
                        .ToList());

        if (articleCommentCounts != null)
        {
            foreach (var listDTO in listDTOs)
            {
                listDTO.CommentCount =
                    articleCommentCounts
                        .Where(x => x.Key == listDTO.Id)
                        .Select(x => x.Value)
                        .FirstOrDefault();
            }
        }

        var result = new PagedResult<ArticleListDTO>
        {
            Items = listDTOs,
            TotalCount = totalCount,
            PageSize = model.Take,
            CurrentPage = model.Skip == 0 && model.Take == 0 ? 1 : (model.Skip / model.Take) + 1
        };

        return result;
    }

    public async Task<ArticleDTO?> GetByIdAsync(Guid id)
    {
        var article = await _repository.GetByIdAsync(id);

        if (article != null)
        {
            List<ArticleCommentDTO> comments = new List<ArticleCommentDTO>();

            // service bus call to get comments
            var sbComments = await _serviceBusService.GetCommentsForArticleAsync(article.Id);

            // service bus call to get commenter user ids
            if (sbComments != null)
            {
                var userIds = sbComments.Select(x => x.UserId).Distinct().ToList();

                var usernames = await _serviceBusService.GetUserNameDetails(userIds);

                foreach (var comment in sbComments)
                {
                    var username = usernames?.Where(x => x.Key == comment.UserId).FirstOrDefault().Value;
                    
                    comments.Add(new ArticleCommentDTO(
                        Id: comment.Id,
                        Content: comment.Content,
                        Username: username ?? null,
                        Published: comment.Published
                    ));
                }
            }
            
            var articleDTO = new ArticleDTO
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                Introduction = article.Introduction,
                Content = article.Content,
                BannerImageUrl = article.BannerImageUrl,
                Created = article.Created,
                Updated = article.Updated,
                Published = article.Published,
                Comments = comments,
                AuthorName = "some author"
            };

            return articleDTO;
        }

        return null;
    }

    public async Task<bool> UpdateAsync(ArticleUpdate model)
    {
        var existingArticle = await _repository.GetByIdAsync(model.Id);

        if (existingArticle == null)
        {
            return false;
        }

        existingArticle.Title = model.Title;
        existingArticle.Description = model.Description;
        existingArticle.Introduction = model.Introduction;
        existingArticle.Content = model.Content;
        existingArticle.BannerImageUrl = model.BannerImageUrl;
        existingArticle.Updated = DateTime.Now;
        existingArticle.UserId = model.AuthorUserId;

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