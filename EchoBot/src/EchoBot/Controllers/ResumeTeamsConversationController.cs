using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using EchoBot.ConversationCache;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace EchoBot.Controllers;

/// <summary>
/// Just to send a follow-up to an existing conversation
/// </summary>
[Route("api/ResumeTeamsConversation")]
public class ResumeTeamsConversationController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly Config _config;
    private readonly IBotConversationCache _botConversationCache;

    public ResumeTeamsConversationController(IBotFrameworkHttpAdapter adapter, Config config, IBotConversationCache botConversationCache)
    {
        this._adapter = adapter;
        this._config = config;
        this._botConversationCache = botConversationCache;
    }

    // POST: api/ResumeTeamsConversation
    [HttpPost]
    public async Task<IActionResult> ResumeTeamsConversation([FromBody] MessageSendRequest messageSendRequest)
    {
        if (messageSendRequest == null || !messageSendRequest.IsValid)
        {
            return BadRequest("Invalid message payload");
        }

        var userCache = _botConversationCache.GetCachedUser(messageSendRequest.UserId);
        if (userCache != null)
        {
            var previousConversationReference = new ConversationReference()
            {
                ChannelId = userCache.ChannelId,
                ServiceUrl = userCache.ServiceUrl,
                Conversation = new ConversationAccount() { Id = userCache.ConversationId },
            };

            var cancellationToken = CancellationToken.None;

            // Ping an update using previous conversation reference (cached conversation ID)
            if (messageSendRequest.MessagePayload != null)
            {

                await ((BotAdapter)_adapter).ContinueConversationAsync(_config.MicrosoftAppId, previousConversationReference,
                    async (turnContext, cancellationToken)
                        => await turnContext.SendActivityAsync(messageSendRequest.MessagePayload, cancellationToken: cancellationToken), cancellationToken);
            }
            else if (messageSendRequest.Card != null)
            {
                var card = new Activity
                {
                    Attachments = new List<Attachment>() { new Attachment { Content = messageSendRequest.Card, ContentType = "application/vnd.microsoft.card.adaptive" } },
                    Type = ActivityTypes.Message
                };

                await ((BotAdapter)_adapter).ContinueConversationAsync(_config.MicrosoftAppId, previousConversationReference,
                    async (turnContext, cancellationToken)
                        => await turnContext.SendActivityAsync(card, cancellationToken: cancellationToken), cancellationToken);
            }

            return Ok();
        }
        return NotFound("User has no cached threads");
    }
}

public class MessageSendRequest
{
    public string UserId { get; set; } = null!; // AAD object ID
    public string? MessagePayload { get; set; } = null;

    public dynamic? Card { get; set; } = null;

    public bool IsValid => !string.IsNullOrEmpty(UserId) && (!string.IsNullOrEmpty(MessagePayload) || Card != null);
}
