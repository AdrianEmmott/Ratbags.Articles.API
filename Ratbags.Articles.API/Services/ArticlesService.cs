﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Ratbags.Shared.DTOs.Events.Events.CommentsRequest;

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

        // TODO - write something in comments that gets the number of comments / article
        
        return articlesDTO.OrderBy(x => x.Created);
    }

    public async Task<ArticleDTO> GetArticleByIdAsync(Guid id)
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

    public async Task<bool> UpdateArticleAsync(ArticleDTO articleDTO)
    {
        var existingArticle = await _repository.GetArticleByIdAsync(articleDTO.Id.Value);

        if (existingArticle == null)
        {
            return false;
        }

        existingArticle.Title = articleDTO.Title;
        existingArticle.Content = articleDTO.Content;
        existingArticle.Updated = DateTime.Now;

        try
        {
            await _repository.UpdateArticleAsync(existingArticle);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError($"Error updating article {articleDTO.Id}: {e.Message}");
            throw;
        }
    }
}