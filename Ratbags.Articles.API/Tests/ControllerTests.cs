using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Controllers;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;

namespace Ratbags.Articles.API.Tests
{
    
    public class ControllerTests
    {
        // DELETE
        [Test]
        public async Task DeleteArticle_NoContent()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var articleId = Guid.NewGuid();

            // act
            var result = await controller.Delete(articleId);

            // asert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteArticle_NotFound()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var articleId = Guid.Empty;

            // act
            var result = await controller.Delete(articleId);

            // asert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteArticle_Exception()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.DeleteArticleAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("test exception"));

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var articleId = Guid.Empty;

            // act
            var result = await controller.Delete(articleId);

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
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(new ArticleDTO());

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var articleId = Guid.NewGuid();

            // act
            var result = await controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task GetArticleNotFound()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            var articleId = Guid.NewGuid();

            // mock return null (article not found)
            mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync((ArticleDTO)null);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            // act
            var result = await controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetArticle_BadRequest()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.GetArticleByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(new ArticleDTO());

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            // no guid
            var articleId = Guid.Empty;

            // act
            var result = await controller.Get(articleId);

            // assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        // GET/{ID}
        //
        //
        // GET (ALL)
        [Test]
        public async Task GetArticles_Ok()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.GetAllArticlesAsync())
                       .ReturnsAsync(new List<ArticleDTO>());

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            // act
            var result = await controller.Get();

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }
        // /GET (ALL)
        //
        //
        // CREATE
        [Test]
        public async Task CreateArticle_NoContent()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.NewGuid());

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created=DateTime.Now,
            };

            // act 
            var result = await controller.Post(article);

            // assert
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task CreateArticle_BadRequest()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.Empty);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act 
            var result = await controller.Post(article);

            // assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }
        [Test]
        public async Task CreateArticle_Exception()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.CreateArticleAsync(It.IsAny<CreateArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await controller.Post(article);

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
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s=>s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(true);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await controller.Put(article);

            // assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateArticle_NotFound()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(false);

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await controller.Put(article);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        public async Task UpdateArticle_Exception()
        {
            // arrange
            var mockService = new Mock<IArticlesService>();
            var mockLogger = new Mock<ILogger<ArticlesController>>();

            mockService.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var controller = new ArticlesController(mockService.Object, mockLogger.Object);

            var article = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await controller.Put(article);

            // assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(500));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while updating the article"));
        }
        // /UPDATE
    }
}
