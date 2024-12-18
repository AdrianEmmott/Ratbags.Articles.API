﻿using MassTransit;
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
    private readonly IMassTransitService _massTransitService;
    private readonly ILogger<ArticlesService> _logger;

    public ArticlesService(
        IArticlesRepository repository,
        IMassTransitService massTransitService,
        ILogger<ArticlesService> logger)
    {
        _repository = repository;
        _massTransitService = massTransitService;
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
            try
            {
                var dto = new ArticleListDTO
                {
                    Id = article.Id,
                    Title = article.Title,
                    Description = article.Description,
                    ThumbnailImageUrl = article.BannerImageUrl ?? string.Empty,
                    CommentCount = await _massTransitService.GetCommentsCountForArticleAsync(article.Id),
                    Published = article.Published
                };

                listDTOs.Add(dto);
            }
            catch (MassTransitException e)
            {
                _logger.LogError($"Error sending comments count request for article {article.Id}: {e.Message} : {e.InnerException}");
                throw;
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
            try
            {
                var comments = await _massTransitService.GetCommentsForArticleAsync(article.Id);

                var articleDTO =  new ArticleDTO
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
                    AuthorName = await _massTransitService.GetUserNameDetailsAsync(article.UserId),
                };

                return articleDTO;
            }
            catch (MassTransitException e)
            {
                _logger.LogError($"Error sending comments request for article {id}: {e.Message}");
                throw;
            }
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