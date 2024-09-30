using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Services;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using Ratbags.Shared.DTOs.Events.DTOs.Articles.Comments;
using Ratbags.Shared.DTOs.Events.Events.CommentsRequest;

namespace Ratbags.Articles.API.Tests
{
    public class ServiceTests
    {
        private Mock<IArticlesRepository> _mockRepository;
        private Mock<IRequestClient<CommentsForArticleRequest>> _mockMassTransitClient;
        private Mock<ILogger<ArticlesService>> _mockLogger;

        private ArticlesService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IArticlesRepository>();
            _mockMassTransitClient = new Mock<IRequestClient<CommentsForArticleRequest>>();
            _mockLogger = new Mock<ILogger<ArticlesService>>();

            _service = new ArticlesService(_mockRepository.Object, _mockMassTransitClient.Object, _mockLogger.Object);
        }

        // CREATE
        [Test]
        public async Task CreateArticleAsync_Success_ReturnsArticleId()
        {
            // arrange
            var createaArticleDTO = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            var articleId = Guid.NewGuid();

            _mockRepository.Setup(r => r.CreateArticleAsync(It.IsAny<Article>()))
                           .ReturnsAsync(articleId);

            // act
            var result = await _service.CreateArticleAsync(createaArticleDTO);

            // assert
            Assert.That(result, Is.EqualTo(articleId));
            _mockRepository.Verify(r => r.CreateArticleAsync(It.IsAny<Article>()), Times.Once);
        }

        // DELETE
        [Test]
        public async Task DeleteArticleAsync_Success()
        {
            // arrange
            var articleId = Guid.NewGuid();
            var article = new Article { Id = articleId, Title = "Test Article" };

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync(article);

            // act
            var result = await _service.DeleteArticleAsync(articleId);

            // assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(r => r.GetArticleByIdAsync(articleId), Times.Once);
            _mockRepository.Verify(r => r.DeleteArticleAsync(articleId), Times.Once);
        }

        [Test]
        public async Task DeleteArticleAsync_Fail_ArticleDoesNotExist()
        {
            // arrange
            var articleId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync((Article)null);

            // act
            var result = await _service.DeleteArticleAsync(articleId);

            // assert
            Assert.That(result, Is.False);
            _mockRepository.Verify(r => r.GetArticleByIdAsync(articleId), Times.Once);
            _mockRepository.Verify(r => r.DeleteArticleAsync(It.IsAny<Guid>()), Times.Never);
        }

        // GET ARTICLE
        [Test]
        public async Task GetArticleByIdAsync_Success()
        {
            // arrange
            var articleId = Guid.NewGuid();
            var article = new Article { Id = articleId, Title = "test title", Content = "test content" };

            var commentsResponse = new CommentsForArticleResponse
            {
                Comments = new List<CommentDTO> { new CommentDTO { Content = "test comment" } }
            };

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync(article);

            _mockMassTransitClient.Setup(m => m.GetResponse<CommentsForArticleResponse>(It.IsAny<CommentsForArticleRequest>(), default, default))
                                  .ReturnsAsync(Mock.Of<Response<CommentsForArticleResponse>>(r => r.Message == commentsResponse));

            // act
            var result = await _service.GetArticleByIdAsync(articleId);

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(articleId));
            Assert.That(result.Comments, Has.Count.EqualTo(1));
            _mockRepository.Verify(r => r.GetArticleByIdAsync(articleId), Times.Once);
            _mockMassTransitClient.Verify(m => m.GetResponse<CommentsForArticleResponse>(It.IsAny<CommentsForArticleRequest>(), default, default), Times.Once);
        }

        [Test]
        public async Task GetArticleByIdAsync_Fail_ArticleDoesNotExist()
        {
            // arrange
            var articleId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync(null as Article);

            // act
            var result = await _service.GetArticleByIdAsync(articleId);

            // assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.GetArticleByIdAsync(articleId), Times.Once);
            _mockMassTransitClient.Verify(m => m.GetResponse<CommentsForArticleResponse>(It.IsAny<CommentsForArticleRequest>(), default, default), Times.Never);
        }

        // GET ARTICLES
        [Test]
        public async Task GetArticlesByIdAsync_Success()
        {
            // arrange
            var articles = new List<Article>
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

            _mockRepository.Setup(r => r.GetAllArticlesAsync())
                           .ReturnsAsync(articles);

            // act
            var result = await _service.GetAllArticlesAsync();

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(2).Items);

            // order by descending (created date)
            Assert.That(result.First().Title, Is.EqualTo("Article 2"));
            Assert.That(result.Last().Title, Is.EqualTo("Article 1"));
        }

        [Test]
        public async Task GetArticlesByIdAsync_Success_NoData()
        {
            // arrange
            var articles = new List<Article>();

            _mockRepository.Setup(r => r.GetAllArticlesAsync())
                           .ReturnsAsync(articles);

            // act
            var result = await _service.GetAllArticlesAsync();

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Exactly(0).Items);
        }

        // UPDATE
        [Test]
        public async Task UpdateArticleAsync_Success()
        {
            // arrange
            var articleId = Guid.NewGuid();

            var existingArticle = new Article
            {
                Id = articleId,
                Title = "Old Title",
                Content = "Old Content",
                Created = DateTime.Now.AddDays(-2),
                Updated = DateTime.Now.AddDays(-1)
            };

            
            // updating
            var  articleDTO = new ArticleDTO
            {
                Id = articleId,
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync(existingArticle);

            _mockRepository.Setup(r => r.UpdateArticleAsync(It.IsAny<Article>()))
                   .Returns(Task.CompletedTask);

            // act
            var result = await _service.UpdateArticleAsync(articleDTO);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateArticleAsync_ThrowsDbUpdateException()
        {
            // arrange
            var articleId = Guid.NewGuid();

            var articleDTO = new ArticleDTO
            {
                Id = articleId,
                Title = "New Title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now
            };

            var existingArticle = new Article
            {
                Id = articleId,
                Title = "Old Title",
                Content = "Old Content",
                Created = DateTime.Now.AddDays(-2),
                Updated = DateTime.Now.AddDays(-1)
            };

            _mockRepository.Setup(r => r.GetArticleByIdAsync(articleId))
                           .ReturnsAsync(existingArticle);

            _mockRepository.Setup(r => r.UpdateArticleAsync(It.IsAny<Article>()))
                           .ThrowsAsync(new DbUpdateException("Error updating article"));

            // play interrupted - act / assert
            var ex = Assert.ThrowsAsync<DbUpdateException>(() => _service.UpdateArticleAsync(articleDTO));
            Assert.That(ex.Message, Is.EqualTo("Error updating article"));
        }
    }
}