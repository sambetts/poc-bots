using System;
using System.Threading.Tasks;
using BastardBot.Common;
using BastardBot.Common.DB;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BastardBot.ScheduledTasks
{
    public static class PublishQnAChanges
    {

        [FunctionName("PublishQnAChanges")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo timerInfo, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = GetConfig(context);
            var brain = new FunctionAppBastardBrain(config);

            await brain.TrainAndPublishNewModel();

        }
        static IConfiguration GetConfig(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
        }
    }
}
