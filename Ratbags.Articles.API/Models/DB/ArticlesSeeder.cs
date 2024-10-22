namespace Ratbags.Articles.API.Models.DB;

public class ArticleSeeder
{
    private readonly ApplicationDbContext _context;

    public ArticleSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public void SeedArticles(int totalRecords, int batchSize = 10000)
    {
        var random = new Random();

        // disable change tracking for improved performance
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var articles = new List<Article>();

        for (int i = 0; i < totalRecords; i++)
        {
            articles.Add(new Article
            {
                Id = Guid.NewGuid(),
                Title = $"Some Title {i + 1}",
                Content = $"Some content for article {i + 1}",
                Created = DateTime.UtcNow,
                Updated =  null,
                Published = null, 
                BannerImageUrl = null
            });

            // batch
            if (articles.Count >= batchSize)
            {
                _context.Articles.AddRange(articles);
                _context.SaveChanges();
                articles.Clear(); // free up memory
            }
        }

        // re-enable change tracking
        _context.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
