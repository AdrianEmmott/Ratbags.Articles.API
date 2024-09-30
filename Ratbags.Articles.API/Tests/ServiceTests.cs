using MassTransit;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Services;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
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
            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            var articleId = Guid.NewGuid();

            _mockRepository.Setup(r => r.CreateArticleAsync(It.IsAny<Article>()))
                           .ReturnsAsync(articleId);

            // act
            var result = await _service.CreateArticleAsync(article);

            // assert
            Assert.That(result, Is.EqualTo(articleId));
            _mockRepository.Verify(r => r.CreateArticleAsync(It.IsAny<Article>()), Times.Once);
        }
        // /CREATE

        // DELETE
        [Test]
        public async Task DeleteArticleAsync_ReturnsTrue_WhenArticleExists()
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
        public async Task DeleteArticleAsync_ReturnsFalse_WhenArticleDoesNotExist()
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
        // DELETE
    }
}
