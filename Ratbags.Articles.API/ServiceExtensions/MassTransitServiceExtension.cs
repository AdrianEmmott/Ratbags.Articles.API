﻿using MassTransit;
using Ratbags.Core.Events.Accounts;
using Ratbags.Core.Events.CommentsRequest;
using Ratbags.Core.Settings;

namespace Ratbags.Articles.API.ServiceExtensions;

public static class MassTransitServiceExtension
{
    public static IServiceCollection AddMassTransitWithRabbitMqServiceExtension(
        this IServiceCollection services, 
        AppSettingsBase appSettings)
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

                cfg.Message<UserFullNameRequest>(c =>
                {
                    c.SetEntityName("accounts.user-full-name"); // set exchange name for this message type
                });

                cfg.Message<CommentsForArticleRequest>(c =>
                {
                    c.SetEntityName("articles.comments"); // set exchange name for this message type
                });

                cfg.Message<CommentsCountForArticleRequest>(c =>
                {
                    c.SetEntityName("articles.comments-count"); // set exchange name for this message type
                });
            });
        });

        return services;
    }
}