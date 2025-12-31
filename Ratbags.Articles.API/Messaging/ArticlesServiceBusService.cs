using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Core.DTOs.Articles;
using Ratbags.Core.Messaging.ASB.RequestReponse;
using System.Text.Json;


namespace Ratbags.Articles.API.Messaging;

public class ArticlesServiceBusService : ServiceBusService<ArticlesServiceBusService>, IArticlesServiceBusService
{
    private readonly ILogger<ArticlesServiceBusService> _logger;
    private readonly AppSettings _appSettings;
    
    public ArticlesServiceBusService(
        AppSettings appSettings,
        ServiceBusClient sbClient,
        ILogger<ArticlesServiceBusService> logger,
        IOptions<JsonSerializerOptions> jsonOptions)
        : base(
            sbClient,
            logger,
            jsonOptions,
            appSettings.Messaging.ASB.ResponseTopic,
            appSettings.Messaging.ASB.ResponseSubscription)
    {
        _logger = logger;
        _appSettings = appSettings;
    }

    public async Task<IEnumerable<CommentCoreDTO>?> GetCommentsForArticleAsync(Guid id)
    {
        try
        {
            // TODO app settings
            var requestTopic = _appSettings.MessagingExtensions.CommentsListTopic;

            var request = new GetCommentsForArticleRequest(articleId: id);

            _logger.LogInformation("Sending get article comments request to {Topic}", requestTopic);
            
            var result = await SendRequestAsync<GetCommentsForArticleRequest, 
                GetCommentsForArticleResponse>(request, requestTopic);

            return result?.comments;
        }
        catch (Exception e)
        {
            _logger.LogError($"Bus error retrieving comments for article {id}: {e.Message}");
            throw;
        }
    }

    public async Task<Dictionary<Guid, int>?> GetArticlesCommentsCount(List<Guid> ids)
    {
        try
        {
            // TODO app settings
            var requestTopic = _appSettings.MessagingExtensions.CommentsCountTopic;

            var request = new GetCommentCountsForArticlesRequest(ArticleIds: ids);
            _logger.LogInformation("Sending get comment counts for articles request to {Topic}", requestTopic);
            var response = await SendRequestAsync<GetCommentCountsForArticlesRequest, GetCommentCountsForArticlesResponse>(request, requestTopic);

            return response?.Counts ?? null;
        }
        catch (Exception e)
        {
            // TODO list the first six article ids
            _logger.LogError($"Bus error retrieving comment counts for {ids.Count()} articles: {e.Message}");
            throw;
        }
    }

    public async Task<Dictionary<Guid, string>?> GetUserNameDetails(IReadOnlyList<Guid> ids)
    {
        try
        {
            var requestTopic = _appSettings.MessagingExtensions.UserNameDetailsTopic;

            var request = new GetUserNameDetailsRequest(userIds: ids);

            _logger.LogInformation("Sending get user name details request to {Topic}", requestTopic);

            var response = await SendRequestAsync<GetUserNameDetailsRequest, GetUserNameDetailsResponse>
                (request, requestTopic);

            return response?.userNameDetails?? null;
        }
        catch (Exception e)
        {
            // TODO list the first six user ids
            _logger.LogError($"Bus error retrieving user name details for {ids.Count()} users: {e.Message}");
            throw;
        }
    }
}