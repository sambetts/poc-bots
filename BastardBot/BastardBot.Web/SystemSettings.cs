using System;

namespace BastardBot.Web
{
    /// <summary>
    /// System config - email settings, URLs, etc
    /// </summary>
    public class SystemSettings
    {
        public SystemSettings(Microsoft.Extensions.Configuration.IConfiguration config)
        {

            var appSettingsConfigSection = config.GetSection("AppSettings");

            // Set config
            this.RecaptchaSecret = appSettingsConfigSection["RecaptchaSecret"];
            this.EmailAddress = appSettingsConfigSection["EmailAddress"];
            this.EmailPassword = appSettingsConfigSection["EmailPassword"];
            string emailPortString = appSettingsConfigSection["EmailPort"];

            string sendEmailSetting = appSettingsConfigSection["SendEmails"];
            if (string.IsNullOrEmpty(sendEmailSetting))
            {
                this.SendEmails = false;
            }
            else
            {
                bool email = false;
                bool.TryParse(sendEmailSetting, out email);
                this.SendEmails = email;
            }


            if (!string.IsNullOrEmpty(emailPortString))
            {
                this.EmailPort = int.Parse(emailPortString);
            }
            else
            {
                ThrowConfigException();
            }
            this.EmailSMTP = appSettingsConfigSection["EmailSMTP"];

            // Mak sure we got everything
            VerifyConfigValues(
                new string[]
                {
                    EmailAddress,
                    EmailPassword,
                    EmailSMTP,
                    RecaptchaSecret
            });
        }

        private static void ThrowConfigException()
        {
            throw new Exception("Missing configuration values");
        }

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

        public string RecaptchaSecret { get; set; }
        public bool SendEmails { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public int EmailPort { get; set; }
        public string EmailSMTP { get; set; }
    }

}
