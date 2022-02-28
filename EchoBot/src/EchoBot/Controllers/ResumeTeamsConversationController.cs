using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace EchoBot.Controllers
{
    [Route("api/ResumeTeamsConversation")]
    public class ResumeTeamsConversationController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly Config _config;
        private readonly BotConversationCache _botConversationCache;

        public ResumeTeamsConversationController(IBotFrameworkHttpAdapter adapter, Config config, BotConversationCache botConversationCache)
        {
            this._adapter = adapter;
            this._config = config;
            this._botConversationCache = botConversationCache;
        }

        // POST: api/ResumeTeamsConversation?userId=48fe59a4-c951-43ca-9d16-972083aa6305
        [HttpPost]
        public async Task<IActionResult> SayHiAsync(string userId)
        {
            var userCache = _botConversationCache.GetCachedUser(userId);
            if (userCache != null)
            {
                var previousConversationReference = new ConversationReference()
                {
                    ChannelId = userCache.ChannelId,
                    Bot = new ChannelAccount() { Id = $"28:{_config.AppCatalogTeamAppId}" },
                    ServiceUrl = userCache.ServiceUrl,
                    Conversation = new ConversationAccount() { Id = userCache.ConversationId },
                };

                var cancellationToken = CancellationToken.None;

                // Ping an update using previous conversation reference (cached conversation ID)
                await ((BotAdapter)_adapter).ContinueConversationAsync(_config.MicrosoftAppId, previousConversationReference,
                    async (turnContext, cancellationToken)
                        => await BotCallback("One time", turnContext, cancellationToken), cancellationToken);

                return Ok();
            }
            return NotFound("User has no cached threads");
        }


        private async Task BotCallback(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(message, cancellationToken: cancellationToken);
        }
    }
}
