using Azure.Messaging.ServiceBus;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Articles.API.Models;
using Ratbags.Core.Messaging.ASB.RequestReponse;

namespace Ratbags.Articles.API.Messaging;

public class ArticlesServiceBusService : ServiceBusService<ArticlesServiceBusService>, IArticlesServiceBusService
{
    private readonly ILogger<ArticlesServiceBusService> _logger;
    private readonly AppSettings _appSettings;

    public ArticlesServiceBusService(
        AppSettings appSettings,
        ServiceBusClient sbClient,
        ILogger<ArticlesServiceBusService> logger)
        : base(
            sbClient,
            logger,
            appSettings.Messaging.ASB.ResponseTopic,
            appSettings.Messaging.ASB.ResponseSubscription)
    {
        _logger = logger;
        _appSettings = appSettings;
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
            _logger.LogError(e, "Bus error retrieving comment counts for {Count} articles", ids.Count);
            throw;
        }
    }

}