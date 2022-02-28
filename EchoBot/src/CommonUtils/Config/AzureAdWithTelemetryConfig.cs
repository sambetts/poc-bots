using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils.Config
{
    public class AzureAdWithTelemetryConfig : PropertyBoundConfig
    {
        public AzureAdWithTelemetryConfig(Microsoft.Extensions.Configuration.IConfiguration config) : base(config)
        {
        }


        [ConfigValue(true)]
        public string AppInsightsInstrumentationKey { get; set; } = string.Empty;

        public bool HaveAppInsightsConfigured => !string.IsNullOrEmpty(AppInsightsInstrumentationKey);

        [ConfigSection("AzureAd")]
        public AzureAdConfig AzureAdConfig { get; set; } = null!;

    }
}
