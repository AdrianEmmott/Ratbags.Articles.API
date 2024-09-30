using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Controllers;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;

namespace Ratbags.Articles.API.Tests
{

    [TestFixture]
    public class ControllerTests
    {
        private Mock<IArticlesService> _mockService;
        private Mock<ILogger<ArticlesController>>_mockLogger;
        private ArticlesController _controller;

        [SetUp] public void SetUp() 
        {
            _mockService = new Mock<IArticlesService>();
            _mockLogger = new Mock<ILogger<ArticlesController>>();
            _controller = new ArticlesController(_mockService.Object, _mockLogger.Object);
        }

        // DELETE
        [Test]
        public async Task DeleteArticle_NoContent()
        {
            // arrange
            _mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var articleId = Guid.NewGuid();

            // act
            var result = await _controller.Delete(articleId);

            // asert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteArticle_NotFound()
        {
            // arrange
            _mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var articleId = Guid.Empty;

            // act
            var result = await _controller.Delete(articleId);

            // asert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteArticle_Exception()
        {
            // arrange
            _mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("test exception"));

            var articleId = Guid.Empty;

            // act
            var result = await _controller.Delete(articleId);

            // asert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while deleting the article"));
        }
        // /DELETE
        //
        //
        // GET/{ID}
        [Test]
        public async Task GetArticle_Ok()
        {
            // arrange
            _mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(new ArticleDTO());

            var articleId = Guid.NewGuid();

            // act
            var result = await _controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task GetArticleNotFound()
        {
            // arrange
            var articleId = Guid.NewGuid();

            // mock return null (article not found)
            _mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync((ArticleDTO)null);

            // act
            var result = await _controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetArticle_BadRequest()
        {
            // arrange
            _mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(new ArticleDTO());

            // no guid
            var articleId = Guid.Empty;

            // act
            var result = await _controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        // GET/{ID}


        // GET (ALL)
        [Test]
        public async Task GetArticles_Ok()
        {
            // arrange
            _mockService.Setup(s => s.GetAllArticlesAsync())
                       .ReturnsAsync(new List<ArticleDTO>());

            // act
            var result = await _controller.Get();

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }
        // /GET (ALL)
        

        // CREATE
        [Test]
        public async Task CreateArticle_NoContent()
        {
            // arrange
            _mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.NewGuid());

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created=DateTime.Now,
            };

            // act 
            var result = await _controller.Post(article);

            // assert
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task CreateArticle_BadRequest()
        {
            // arrange
            _mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.Empty);

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act 
            var result = await _controller.Post(article);

            // assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        [Test]
        public async Task CreateArticle_Exception()
        {
            // arrange
            _mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Post(article);

            // asert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while creating the article"));
        }
        // /CREATE
        //
        //
        // UPDATE
        [Test]
        public async Task UpdateArticle_NoContent()
        {
            // arrange
            _mockService.Setup(s=>s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(true);

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Put(article);

            // assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateArticle_NotFound()
        {
            // arrange
            _mockService.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(false);

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Put(article);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        public async Task UpdateArticle_Exception()
        {
            // arrange
            _mockService.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Put(article);

            // assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while updating the article"));
        }
        // /UPDATE
    }
}
