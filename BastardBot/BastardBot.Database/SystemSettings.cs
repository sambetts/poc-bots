using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace BastardBot.Common
{
    /// <summary>
    /// Settings used for solution
    /// </summary>
    public class SystemSettings
    {
        #region Constructors

        public SystemSettings(IConfiguration config) : this(config, true)
        {
        }
        public SystemSettings(IConfiguration config, bool validateNullConfig)
        {
            //var appSettingsConfigSection = config.GetSection("AppSettings");

            // Set config
            this.QnAMakerHost = config["QnAMakerHost"];
            this.QnASubscriptionKey = config["QnASubscriptionKey"];

            this.QnAEndpointHostName = config["QnAEndpointHostName"];
            this.QnAEndpointKey = config["QnAEndpointKey"];
            this.QnAKnowledgebaseId = config["QnAKnowledgebaseId"];

            // Mak sure we got everything
            if (validateNullConfig)
            {
                VerifyConfigValues(
                    new string[]
                    {
                        QnAMakerHost,
                        QnASubscriptionKey
                    });
            }

        }
        #endregion

        #region Config Verification

        static void VerifyConfigValues(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrEmpty(args[i]))
                {
                    ThrowConfigException();
                }
            }
        }
        private static void ThrowConfigException()
        {
            throw new ApplicationException("Missing configuration values");
        }
        #endregion

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// URL of endpoint
        /// </summary>
        public string QnAMakerHost { get; set; }
        public string QnASubscriptionKey { get; set; }
        public string QnAEndpointKey { get; set; }
        public string QnAEndpointHostName { get; set; }
        public string QnAKnowledgebaseId { get; set; }
    }
}
