using MassTransit;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Messaging.Requests;
using Ratbags.Articles.API.Messaging.Responses;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.Accounts;
using Ratbags.Core.Events.CommentsRequest;
using System.Text.Json;
using System.Xml.Linq;

namespace Ratbags.Articles.API.Services
{
    public class MassTransitService : IMassTransitService
    {
        private readonly IRequestClient<PublishCommentsRequestMessage> _client;
        private readonly IRequestClient<CommentsForArticleRequest> _massTrasitCommentsClient;
        private readonly IRequestClient<CommentsCountForArticleRequest> _massTrasitClientCommentsCountClient;
        private readonly IRequestClient<UserFullNameRequest> _massTrasitUserNameDetailsClient;

        private readonly IBus _bus;

        public MassTransitService(

            IRequestClient<PublishCommentsRequestMessage> client,
            IRequestClient<CommentsForArticleRequest> massTrasitCommentsClient,
            IRequestClient<CommentsCountForArticleRequest> massTrasitClientCommentsCountClient,
            IRequestClient<UserFullNameRequest> massTrasitUserNameDetailsClient,
            IBus bus)
        {
            _client = client;
            _massTrasitCommentsClient = massTrasitCommentsClient;
            _massTrasitClientCommentsCountClient = massTrasitClientCommentsCountClient;
            _massTrasitUserNameDetailsClient = massTrasitUserNameDetailsClient;
            _bus = bus;
        }

        public async Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id)
        {
            var requestJson = JsonSerializer.Serialize(new { ArticleId = id });
            var message = new PublishCommentsRequestMessage { Message = requestJson };

            await _bus.Publish(message, ctx =>
            {
                ctx.SetRoutingKey("comments.request");
            });

            var response = await _client.GetResponse<JsonResponseMessage>(message);
            var jsonResponse = (string)response.Message.JsonResponse;

            //var response = await _massTrasitCommentsClient
            //                    .GetResponse<CommentsForArticleResponse>
            //                    (new CommentsForArticleRequest
            //                    {
            //                        ArticleId = id
            //                    });
            
            var ds =  JsonSerializer.Deserialize<List<CommentDTO>>(jsonResponse);

            return ds;
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
            var response = await _massTrasitUserNameDetailsClient
                            .GetResponse<UserFullNameResponse>
                            (new UserFullNameRequest
                            {
                                UserId = id
                            });

            return response.Message.FullName;
        }
    }
}
