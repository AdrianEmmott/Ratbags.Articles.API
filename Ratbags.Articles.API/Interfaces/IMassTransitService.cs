using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Interfaces;

public interface IMassTransitService
{
    Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id);
    Task<int> GetCommentsCountForArticleAsync(Guid id);

    Task<string> GetUserNameDetailsAsync(Guid id);
}
