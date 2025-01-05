using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Interfaces
{
    public interface IServiceBusService
    {
        Task<int> GetCommentsCountForArticleAsync(Guid id);
        Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id);
        Task<string> GetUserNameDetailsAsync(Guid id);
    }
}