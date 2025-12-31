using Ratbags.Articles.API.Models.DTOs;
using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Interfaces
{
    public interface IArticlesServiceBusService
    {
        Task<Dictionary<Guid, int>?> GetArticlesCommentsCount(List<Guid> ids);

        Task<IEnumerable<CommentCoreDTO>?> GetCommentsForArticleAsync(Guid id);

        Task<Dictionary<Guid, string>?> GetUserNameDetails(IReadOnlyList<Guid> userIds);
    }
}