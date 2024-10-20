using MassTransit;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.Accounts;
using Ratbags.Core.Events.CommentsRequest;

namespace Ratbags.Articles.API.Services
{
    public class MassTransitService : IMassTransitService
    {
        private readonly IRequestClient<CommentsForArticleRequest> _massTrasitCommentsClient;
        private readonly IRequestClient<CommentsCountForArticleRequest> _massTrasitClientCommentsCountClient;
        private readonly IRequestClient<UserFullNameRequest> _massTrasitUserNameDetailsClient;

        public MassTransitService(
            IRequestClient<CommentsForArticleRequest> massTrasitCommentsClient, 
            IRequestClient<CommentsCountForArticleRequest> massTrasitClientCommentsCountClient,
            IRequestClient<UserFullNameRequest> massTrasitUserNameDetailsClient)
        {
            _massTrasitCommentsClient = massTrasitCommentsClient;
            _massTrasitClientCommentsCountClient = massTrasitClientCommentsCountClient;
            _massTrasitUserNameDetailsClient = massTrasitUserNameDetailsClient;
        }

        public async Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id)
        {
            var response = await _massTrasitCommentsClient
                                .GetResponse<CommentsForArticleResponse>
                                (new CommentsForArticleRequest
                                {
                                    ArticleId = id
                                });

            return response.Message.Comments;
        }

        public async Task<int> GetCommentsCountForArticleAsync(Guid id)
        {
            var response = await _massTrasitClientCommentsCountClient
                                    .GetResponse<CommentsCountForArticleResponse>
                                    (new CommentsCountForArticleRequest
                                    {
                                        ArticleId = id
                                    });

            return response.Message.Count;
        }

        public async Task<string> GetUserNameDetailsAsync(Guid id)
        {
            try
            {
                var response = await _massTrasitUserNameDetailsClient
                                .GetResponse<UserFullNameResponse>
                                (new UserFullNameRequest
                                {
                                    UserId = id
                                });

                return response.Message.FullName;

            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
