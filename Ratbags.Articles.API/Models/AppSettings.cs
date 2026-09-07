using Microsoft.Identity.Client;
using Ratbags.Core.Settings;

namespace Ratbags.Articles.API.Models
{
    public class AppSettings : AppSettingsBase
    {
        public MessagingExtensions MessagingExtensions { get; set; } = default!;
        public Services Services { get; set; } = default!;
    }
    public class MessagingExtensions
    {
        public string CommentsCountTopic { get; set; } = default!;
    }
    public class Services
    {
        public string AccountsApi { get; set; } = default!;
    }
}
