using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Articles.API.Services;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.CommentsRequest;
using Ratbags.Core.Models.Articles;

namespace Ratbags.Articles.API.Tests;

public class ServiceTests
{
    private Mock<IArticlesRepository> _mockRepository;
    private Mock<IMassTransitService> _mockMassTransitService;
    private Mock<ILogger<ArticlesService>> _mockLogger;

    private ArticlesService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IArticlesRepository>();
        _mockMassTransitService = new Mock<IMassTransitService>();
        _mockLogger = new Mock<ILogger<ArticlesService>>();

        _service = new ArticlesService(
            _mockRepository.Object,
            _mockMassTransitService.Object,
            _mockLogger.Object);
    }

    // CREATE
    [Test]
    public async Task CreateArticleAsync_Success()
    {
        // arrange
        var model = new CreateArticleModel
        {
            Title = "An article title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now,
        };

        var articleId = Guid.NewGuid();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Article>()))
                       .ReturnsAsync(articleId);

        // act
        var result = await _service.CreateAsync(model);

        // assert
        Assert.That(result, Is.EqualTo(articleId));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public void CreateAsync_Exception()
    {
        // arrange
        var model = new CreateArticleModel
        {
            Title = "An article title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now,
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Article>()))
                       .ThrowsAsync(new DbUpdateException("Error creating article"));

        // play interrupted - act / assert
        var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.CreateAsync(model));
        Assert.That(ex.Message, Is.EqualTo("Error creating article"));
    }


    // DELETE
    [Test]
    public async Task DeleteArticleAsync_Success()
    {
        // arrange
        var id = Guid.NewGuid();

        // in db
        var model = new Article
        {
            Id = id,
            Title = "Test Article"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(model);

        // act
        var result = await _service.DeleteAsync(id);

        // assert
        Assert.That(result, Is.True);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Test]
    public async Task DeleteArticleAsync_Fail_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(null as Article);

        // act
        var result = await _service.DeleteAsync(id);

        // assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void DeleteAsync_Exception()
    {
        // arrange
        var id = Guid.NewGuid();

        // in db
        var model = new Article
        {
            Id = id,
            Title = "Test Article"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(model);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                       .ThrowsAsync(new DbUpdateException("Error deleting article"));

        // play interrupted - act / assert
        var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.DeleteAsync(id));
        Assert.That(ex.Message, Is.EqualTo("Error deleting article"));
    }


    // GET ARTICLE
    [Test]
    public async Task GetArticleByIdAsync_Success()
    {
        // arrange
        var id = Guid.NewGuid();
        var article = new Article
        {
            Id = id,
            Title = "test title",
            Content = "test content"
        };

        var comments = new List<CommentDTO>
        {
            new CommentDTO
            {
                Id = Guid.NewGuid(),
                Content = "some comment 1",
                Published = DateTime.UtcNow.AddDays(-2)
            },
            new CommentDTO
            {
                Id = Guid.NewGuid(),
                Content = "some comment 2",
                Published = DateTime.UtcNow.AddDays(-1)
            },
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync(article);

        _mockMassTransitService.Setup(m => m.GetCommentsForArticleAsync(It.IsAny<Guid>()))
                .ReturnsAsync(comments);

        // act
        var result = await _service.GetByIdAsync(id);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Comments, Has.Count.EqualTo(2));

        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Test]
    public async Task GetArticleByIdAsync_Fail_ArticleDoesNotExist()
    {
        // arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(null as Article);

        // act
        var result = await _service.GetByIdAsync(id);

        // assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }


    // GET ARTICLES
    [Test]
    public async Task GetArticlesAsync_Success()
    {
        // arrange
        // in db
        var modelList = new List<Article>
        {
            new Article {
                Id = Guid.NewGuid(),
                Title = "article 2",
                Description="some desc 2",
                Created = DateTime.Now.AddDays(-1),
                Updated = DateTime.Now.AddDays(-1),
                Published = DateTime.Now
            },
            new Article {
                Id = Guid.NewGuid(),
                Title = "article 1",
                Description="some desc 1",
                Created = DateTime.Now.AddDays(-2),
                Updated = DateTime.MinValue,
                Published = DateTime.MinValue
            }
        };

        var model = new GetArticlesParameters { Skip = 0, Take = 0 };

        _mockRepository.Setup(r => r.GetArticlesAsync(model))
                      .ReturnsAsync((modelList, modelList.Count));

        _mockMassTransitService.Setup(m => m.GetCommentsCountForArticleAsync(It.IsAny<Guid>()))
           .ReturnsAsync(modelList.Count);

        // act
        var result = await _service.GetAsync(model);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Has.Exactly(2).Items);
    }

    [Test]
    public async Task GetArticlesAsync_Success_NoData()
    {
        // arrange
        var model = new GetArticlesParameters { Skip = 0, Take = 0 };

        var modelList = new List<Article>();

        _mockRepository.Setup(r => r.GetArticlesAsync(model))
                       .ReturnsAsync((modelList, modelList.Count));

        // act
        var result = await _service.GetAsync(model);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Has.Exactly(0).Items);
    }


    // UPDATE
    [Test]
    public async Task UpdateArticleAsync_Success()
    {
        // arrange
        var id = Guid.NewGuid();

        // article in db
        var existingArticle = new Article
        {
            Id = id,
            Title = "Old Title",
            Content = "Old Content",
            Created = DateTime.Now.AddDays(-2),
            Updated = DateTime.Now.AddDays(-1)
        };

        // update article dto
        var model = new UpdateArticleModel
        {
            Id = id,
            Title = "New article title",
            Content = "<p>lorem ipsum</p>",
            Updated = DateTime.Now,
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(existingArticle);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Article>()))
               .Returns(Task.CompletedTask);

        // grab article to check new values
        Article updatedModel = new Article();
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Article>()))
                       .Callback<Article>(article => updatedModel = article)
                       .Returns(Task.CompletedTask);

        // act
        var result = await _service.UpdateAsync(model);

        // assert
        Assert.That(result, Is.True);
        Assert.That(updatedModel.Title, Is.EqualTo(model.Title));
        Assert.That(updatedModel.Content, Is.EqualTo(model.Content));
    }

    [Test]
    public void UpdateArticleAsync_Exception()
    {
        // arrange
        var articleId = Guid.NewGuid();

        // article in db
        var existingArticle = new Article
        {
            Id = articleId,
            Title = "Old Title",
            Content = "Old Content",
            Created = DateTime.Now.AddDays(-2),
            Updated = DateTime.Now.AddDays(-1)
        };

        // update article dto
        var model = new UpdateArticleModel
        {
            Id = articleId,
            Title = "New Title",
            Content = "<p>lorem ipsum</p>",
            Updated = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(articleId))
                       .ReturnsAsync(existingArticle);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Article>()))
                       .ThrowsAsync(new DbUpdateException("Error updating article"));

        // play interrupted - act / assert
        var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.UpdateAsync(model));
        Assert.That(ex.Message, Is.EqualTo("Error updating article"));
    }
}