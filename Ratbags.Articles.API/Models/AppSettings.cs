using Ratbags.Core.Settings;

namespace Ratbags.Articles.API.Models
{
    public class AppSettings : AppSettingsBase
    {
        public string AZSBTestConnection { get; set; } = default!;
    }
}
