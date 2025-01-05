using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Ratbags.Articles.API.Interfaces;
using Ratbags.Core.DTOs.Articles;

namespace Ratbags.Articles.API.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _sbClient;
    private readonly ILogger<ServiceBusService> _logger;

    public ServiceBusService(
        ServiceBusClient sbClient,
        ILogger<ServiceBusService> logger)
    {
        _sbClient = sbClient;
        _logger = logger;
    }

    public async Task<List<CommentDTO>> GetCommentsForArticleAsync(Guid id)
    {
        // topics names and subscription
        var requestTopic = "comments-topic";
        var responseTopic = "comments-response-topic";
        var responseSubscription = "articles-service-subscription";
        
        // create sender and receiver
        var sender = _sbClient.CreateSender(requestTopic);
        var receiver = _sbClient.CreateReceiver(responseTopic, responseSubscription,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        // create a unique correlation id
        var correlationId = Guid.NewGuid().ToString();

        // build request message
        var requestMessage = new ServiceBusMessage(id.ToString())
        {
            CorrelationId = correlationId,
            ReplyTo = responseTopic // tells comments api where to send response
        };

        // send request
        _logger.LogInformation("sending request to comments-topic with CorrelationId {CorrelationId}", correlationId);
        await sender.SendMessageAsync(requestMessage);

        // await response
        ServiceBusReceivedMessage? responseMessage = null;
        var maxWaitTime = TimeSpan.FromSeconds(30);
        var watch = System.Diagnostics.Stopwatch.StartNew();

        while (responseMessage == null && watch.Elapsed < maxWaitTime)
        {
            // attempt to receive up to 1 message
            var messages = await receiver.ReceiveMessagesAsync(
                maxMessages: 1,
                maxWaitTime: TimeSpan.FromSeconds(3));

            // looking for a message with our correlation id
            responseMessage = messages.FirstOrDefault(m => m.CorrelationId == correlationId);
        }

        if (responseMessage == null)
        {
            _logger.LogWarning("No response received within {MaxWaitTime} seconds.", maxWaitTime.TotalSeconds);
            return new List<CommentDTO>();
        }

        // dserialize response into a list of comments
        var bodyString = responseMessage.Body.ToString();
        var comments = JsonSerializer.Deserialize<List<CommentDTO>>(bodyString);

        // complete response message to remove it from subscription
        await receiver.CompleteMessageAsync(responseMessage);

        _logger.LogInformation("Received response with {Count} comments for Article {ArticleId}",
            comments?.Count ?? 0, id);

        return comments ?? new List<CommentDTO>();
    }

    // TODO
    public async Task<int> GetCommentsCountForArticleAsync(Guid id)
    {
        return 0;
    }

    public async Task<string> GetUserNameDetailsAsync(Guid id)
    {
        return string.Empty;
    }
}