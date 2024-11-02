using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Articles.API.Repositories;

namespace Ratbags.Articles.API.Tests;

public class RepositoryTests
{
    private Mock<ILogger<ArticlesRepository>> _mockLogger;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ArticlesRepository>>();
    }

    [Test]
    public async Task GetArticlesAsync_Success_OrderDescendingCreated()
    {
        // arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Articles.AddRange(
                new Article { 
                    Id = Guid.NewGuid(), 
                    Title = "article 1", 
                    Content="some content 1", 
                    Description="some desc 1",
                    Created = DateTime.Now.AddDays(-2) 
                },
                new Article { 
                    Id = Guid.NewGuid(), 
                    Title = "article 2", 
                    Content = "some content 2",
                    Description = "some desc 2",
                    Created = DateTime.Now.AddDays(-1) 
                }
            );

            await context.SaveChangesAsync();

            var repository = new ArticlesRepository(context, _mockLogger.Object);
            var paramModel = new GetArticlesParameters { Skip = 0, Take = 0 };

            // act
            var result = await repository.GetArticlesAsync(paramModel);

            // assert
            Assert.That(result.Articles, Has.Exactly(2).Items);

            // assert descending created order 
            Assert.That(result.Articles.First().Title, Is.EqualTo("article 2"));
            Assert.That(result.Articles.Last().Title, Is.EqualTo("article 1"));
        }
    }
}