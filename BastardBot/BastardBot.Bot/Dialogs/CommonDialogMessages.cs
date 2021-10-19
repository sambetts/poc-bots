using BastardBot.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace BastardBot.Bot.Dialogs
{
    public class CommonDialogMessages
    {
        public static async Task SendConfirmQnACard(WaterfallStepContext stepContext, CancellationToken cancellationToken, NewInsultQnA newInsultQnA)
        {
            // Send adaptive card with new QnA
            var adaptiveCard = CardGenerator.GetNewQnACard(newInsultQnA);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = adaptiveCard,
            };
            var promptMessageSummary = MessageFactory.Attachment(adaptiveCardAttachment, $"Want to add any other insults I should reply '{newInsultQnA.InsultResponse}' to?");
            await stepContext.Context.SendActivityAsync(promptMessageSummary, cancellationToken);

        }
    }
}
