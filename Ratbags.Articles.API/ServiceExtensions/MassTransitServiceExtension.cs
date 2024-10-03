using MassTransit;
using Ratbags.Shared.DTOs.Events.AppSettingsBase;
using Ratbags.Shared.DTOs.Events.Events.CommentsRequest;

namespace Ratbags.Articles.API.ServiceExtensions
{
    public static class MassTransitServiceExtension
    {
        public static IServiceCollection AddMassTransitWithRabbitMqServiceExtension(this IServiceCollection services, AppSettingsBase appSettings)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host($"rabbitmq://{appSettings.Messaging.Hostname}/{appSettings.Messaging.VirtualHost}", h =>
                    {
                        h.Username(appSettings.Messaging.Username);
                        h.Password(appSettings.Messaging.Password);
                    });

                    // ensure messages sent to correct exchange and routing key
                    cfg.Message<CommentsForArticleRequest>(c =>
                    {
                        c.SetEntityName("articles.comments.exchange"); // set exchange name for this message type
                    });

                    // give the internal bus a meaningful name
                    cfg.ReceiveEndpoint("articles_comments_api_internal_bus_endpoint", q =>
                    {
                        // disable the auto-creation of the default queue exchange
                        q.ConfigureConsumeTopology = false; // prevents creating an exchange with the same name as the queue
                    });

                    // spare code! pointless but keep for now in case you move away from request/response
                    cfg.Send<CommentsForArticleRequest>(x =>
                    {
                        x.UseRoutingKeyFormatter(context => "comments.request");
                    });
                });
            });

            return services;
        }
    }
}
