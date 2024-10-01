using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Ratbags.Articles.API.Controllers;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Shared.DTOs.Events.DTOs.Articles;
using System.Net;

namespace Ratbags.Articles.API.Tests
{

    [TestFixture]
    public class ControllerTests
    {
        private Mock<IService> _mockService;
        private Mock<ILogger<ArticlesController>> _mockLogger;

        private ArticlesController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IService>();
            _mockLogger = new Mock<ILogger<ArticlesController>>();
            _controller = new ArticlesController(_mockService.Object, _mockLogger.Object);
        }

        // DELETE
        [Test]
        public async Task DeleteArticle_NoContent()
        {
            // arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(id);

            // assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task DeleteArticle_NotFound()
        {
            // arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(id);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteArticle_Exception()
        {
            // arrange
            var id = Guid.Empty;

            _mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("test exception"));

            // act
            var result = await _controller.Delete(id);

            // assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while deleting the article"));
        }


        // GET/{ID}
        [Test]
        public async Task GetArticle_Ok()
        {
            // arrange
            var dto = new ArticleDTO
            {
                Id = Guid.NewGuid(),
            };

            _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(dto);

            // act
            var result = await _controller.Get(dto.Id);

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(dto));
        }

        [Test]
        public async Task GetArticle_NotFound()
        {
            // arrange
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(null as ArticleDTO);

            // act
            var result = await _controller.Get(id);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task GetArticle_BadRequest()
        {
            // arrange
            _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(new ArticleDTO());

            // act
            var result = await _controller.Get(Guid.Empty);

            // assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }


        // GET
        [Test]
        public async Task GetArticles_Ok()
        {
            // arrange
            _mockService.Setup(s => s.GetAsync())
                       .ReturnsAsync(new List<ArticleDTO>());

            // act
            var result = await _controller.Get();

            // assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }


        // POST
        [Test]
        public async Task CreateArticle_NoContent()
        {
            // arrange
            _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.NewGuid());

            var dto = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act 
            var result = await _controller.Post(dto);

            // assert
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task CreateArticle_BadRequest()
        {
            // arrange
            _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateArticleDTO>()))
                       .ReturnsAsync(Guid.Empty);

            var dto = new CreateArticleDTO
            {
                Title = string.Empty,
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act 
            var result = await _controller.Post(dto);

            // assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CreateArticle_Exception()
        {
            // arrange
            _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var dto = new CreateArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Post(dto);

            // assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, 
                Is.EqualTo((int)HttpStatusCode.InternalServerError));

            Assert.That(statusCodeResult.Value, 
                Is.EqualTo("An error occurred while creating the article"));
        }


        // PUT
        [Test]
        public async Task Put_NoContent()
        {
            // arrange
            var dto = new ArticleDTO
            {
                Id = Guid.NewGuid(),
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            _mockService.Setup(s => s.UpdateAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Put(dto);

            // assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task Put_NotFound()
        {
            // arrange
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<ArticleDTO>()))
                .ReturnsAsync(false);

            var dto = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Put(dto);

            // assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        public async Task Put_Exception()
        {
            // arrange
            _mockService.Setup(s => s.UpdateAsync(It.IsAny<ArticleDTO>()))
                .ThrowsAsync(new Exception("test exception"));

            var dto = new ArticleDTO
            {
                Title = "An article title",
                Content = "<p>lorem ipsum</p>",
                Created = DateTime.Now,
            };

            // act
            var result = await _controller.Put(dto);

            // assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(statusCodeResult.Value, Is.EqualTo("An error occurred while updating the article"));
        }
    }
}
