using Microsoft.EntityFrameworkCore;
using Ratbags.Accounts.Client;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Messaging;
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
    private readonly IAccountsClient _accountsClient;
    private readonly ILogger<ArticlesService> _logger;

    public ArticlesService(
        IArticlesRepository repository,
        IArticlesServiceBusService serviceBusService,
        IAccountsClient accountsClient,
        ILogger<ArticlesService> logger)
    {
        _repository = repository;
        _serviceBusService = serviceBusService;
        _accountsClient = accountsClient;
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

        //var articleCommentCounts =
        //            await _serviceBusService
        //                .GetArticlesCommentsCount(articles.Select(x => x.Id)
        //                .ToList());

        //if (articleCommentCounts != null)
        //{
        //    foreach (var listDTO in listDTOs)
        //    {
        //        listDTO.CommentCount =
        //            articleCommentCounts
        //                .Where(x => x.Key == listDTO.Id)
        //                .Select(x => x.Value)
        //                .FirstOrDefault();
        //    }
        //}

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
            // get author username -
            // TODO - using the same lookup which only takes a list of user ids is... alright
            // could have multiple authors in the future
            var authorUserIds = new List<Guid>();
            authorUserIds.Add(article.UserId);
            var authorUsername = await _accountsClient.GetUsernamesAsync(authorUserIds);

            // comments are no longer fetched here - moving to a dedicated http call to
            // Comments.API (was an ASB call via GetCommentsForArticleAsync, since removed)

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
                // authorUsername may not contain an entry at all if the account no
                // longer exists - don't assume .First() will always have something
                AuthorName = authorUsername?.Values.FirstOrDefault() ?? "unknown author"
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
