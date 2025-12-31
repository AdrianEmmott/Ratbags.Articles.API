using Microsoft.Identity.Client;
using Ratbags.Core.Settings;

namespace Ratbags.Articles.API.Models
{
    public class AppSettings : AppSettingsBase
    {
        public MessagingExtensions MessagingExtensions { get; set; } = default!;
    }
    public class MessagingExtensions
    {
        public string CommentsListTopic { get; set; } = default!;
        public string CommentsCountTopic { get; set; } = default!;
        public string UserNameDetailsTopic { get; set; } = default!;
    }
}
