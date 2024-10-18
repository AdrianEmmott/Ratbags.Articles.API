using MassTransit;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.CommentsRequest;

namespace Ratbags.Articles.API.Interfaces;

public interface IMassTransitService
{
    Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id);
    Task<int> GetCommentsCountForArticleAsync(Guid id);
}
