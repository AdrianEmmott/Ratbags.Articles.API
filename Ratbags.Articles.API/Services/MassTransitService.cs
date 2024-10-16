﻿using MassTransit;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Events.CommentsRequest;

namespace Ratbags.Articles.API.Services
{
    public class MassTransitService : IMassTransitService
    {
        private readonly IRequestClient<CommentsForArticleRequest> _massTrasitCommentsClient;
        private readonly IRequestClient<CommentsCountForArticleRequest> _massTrasitClientCommentsCountClient;

        public MassTransitService(
            IRequestClient<CommentsForArticleRequest> massTrasitCommentsClient, 
            IRequestClient<CommentsCountForArticleRequest> massTrasitClientCommentsCountClient)
        {
            _massTrasitCommentsClient = massTrasitCommentsClient;
            _massTrasitClientCommentsCountClient = massTrasitClientCommentsCountClient;
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
    }
}
