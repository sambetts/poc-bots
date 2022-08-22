using CommonUtils.Config;
using Microsoft.Extensions.Configuration;

namespace EchoBot
{
    public class Config : PropertyBoundConfig
    {
        public Config(IConfiguration config) : base(config)
        {
        }


        [ConfigValue(true)]
        public string MicrosoftAppId { get; set; } = string.Empty;
        [ConfigValue(true)]
        public string MicrosoftAppPassword { get; set; } = string.Empty;
        [ConfigValue(true)]
        public string MicrosoftAppTenantId { get; set; } = string.Empty;

        [ConfigValue(true)]
        public string AppInsightsInstrumentationKey { get; set; }

        [ConfigValue(true)]
        public string Storage { get; set; }

    }
}
