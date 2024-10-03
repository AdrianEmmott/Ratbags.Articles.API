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

                    cfg.Message<CommentsForArticleRequest>(c =>
                    {
                        c.SetEntityName("articles.comments"); // set exchange name for this message type
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