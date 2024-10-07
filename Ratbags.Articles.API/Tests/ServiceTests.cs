using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Services;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.DTOs.Articles.Comments;
using Ratbags.Core.Events.CommentsRequest;

namespace Ratbags.Articles.API.Tests;

public class ServiceTests
{
    private Mock<IRepository> _mockRepository;
    private Mock<IRequestClient<CommentsForArticleRequest>> _mockMassTransitClient;
    private Mock<ILogger<Service>> _mockLogger;

    private Service _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepository>();
        _mockMassTransitClient = new Mock<IRequestClient<CommentsForArticleRequest>>();
        _mockLogger = new Mock<ILogger<Service>>();

        _service = new Service(_mockRepository.Object, _mockMassTransitClient.Object, _mockLogger.Object);
    }

    // CREATE
    [Test]
    public async Task CreateArticleAsync_Success()
    {
        // arrange
        var dto = new CreateArticleDTO
        {
            Title = "An article title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now,
        };

        var articleId = Guid.NewGuid();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Article>()))
                       .ReturnsAsync(articleId);

        // act
        var result = await _service.CreateAsync(dto);

        // assert
        Assert.That(result, Is.EqualTo(articleId));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public void CreateAsync_Exception()
    {
        // arrange
        var dto = new CreateArticleDTO
        {
            Title = "An article title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now,
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Article>()))
                       .ThrowsAsync(new DbUpdateException("Error creating article"));

        // play interrupted - act / assert
        var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.CreateAsync(dto));
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

        var response = new CommentsForArticleResponse
        {
            Comments = new List<CommentDTO> 
            {
                new CommentDTO 
                { 
                    Content = "test comment" 
                } 
            }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(article);

        _mockMassTransitClient.Setup(m => m.GetResponse<CommentsForArticleResponse>
            (It.IsAny<CommentsForArticleRequest>(), default, default))
            .ReturnsAsync(Mock.Of<Response<CommentsForArticleResponse>>
                (r => r.Message == response));

        // act
        var result = await _service.GetByIdAsync(id);

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Comments, Has.Count.EqualTo(1));

        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockMassTransitClient.Verify(m => m.GetResponse<CommentsForArticleResponse>
            (It.IsAny<CommentsForArticleRequest>(), default, default), Times.Once);
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
        _mockMassTransitClient.Verify(m => m.GetResponse<CommentsForArticleResponse>
            (It.IsAny<CommentsForArticleRequest>(), default, default), Times.Never);
    }


    // GET ARTICLES
    [Test]
    public async Task GetArticlesByIdAsync_Success()
    {
        // arrange
        // in db
        var modelList = new List<Article>
        {
            new Article { 
                Id = Guid.NewGuid(), 
                Title = "Article 1", 
                Created = DateTime.Now.AddDays(-2), 
                Updated = DateTime.MinValue, 
                Published = DateTime.MinValue
            },
            new Article { 
                Id = Guid.NewGuid(), 
                Title = "Article 2", 
                Created = DateTime.Now.AddDays(-1), 
                Updated = DateTime.Now.AddDays(-1), 
                Published = DateTime.Now 
            }
        };

        _mockRepository.Setup(r => r.GetAsync())
                       .ReturnsAsync(modelList);

        // act
        var result = await _service.GetAsync();

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Exactly(2).Items);

        // order by descending (created date)
        Assert.That(result.First().Title, Is.EqualTo(modelList.Last().Title));
        Assert.That(result.Last().Title, Is.EqualTo(modelList.First().Title));
    }

    [Test]
    public async Task GetArticlesByIdAsync_Success_NoData()
    {
        // arrange
        var modelList = new List<Article>();

        _mockRepository.Setup(r => r.GetAsync())
                       .ReturnsAsync(modelList);

        // act
        var result = await _service.GetAsync();

        // assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Exactly(0).Items);
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
        var dto = new ArticleDTO
        {
            Id = id,
            Title = "New article title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now,
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
        var result = await _service.UpdateAsync(dto);

        // assert
        Assert.That(result, Is.True);
        Assert.That(updatedModel.Title, Is.EqualTo(dto.Title));
        Assert.That(updatedModel.Content, Is.EqualTo(dto.Content));
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
        var dto = new ArticleDTO
        {
            Id = articleId,
            Title = "New Title",
            Content = "<p>lorem ipsum</p>",
            Created = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(articleId))
                       .ReturnsAsync(existingArticle);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Article>()))
                       .ThrowsAsync(new DbUpdateException("Error updating article"));

        // play interrupted - act / assert
        var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.UpdateAsync(dto));
        Assert.That(ex.Message, Is.EqualTo("Error updating article"));
    }
}