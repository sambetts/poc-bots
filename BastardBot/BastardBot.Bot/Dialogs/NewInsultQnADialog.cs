using BastardBot.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BastardBot.Bot.Dialogs
{
    public class NewInsultQnADialog : ComponentDialog
    {
        private BastardBrain _brain;
        public NewInsultQnADialog(BastardBrain brain) : base(nameof(NewInsultQnADialog))
        {
            _brain = brain;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NewInsult,
                PromptReplyToInsult,
                AddInsultResponse,
                MoreResponses,
                ConfirmNewInsultQnA
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt), InsultResponseValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AddMoreInsultsForResponseDialog(_brain.GetBastardDBContext()));
        }


        private async Task<DialogTurnResult> NewInsult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Options;

            // Save new QnA to session
            stepContext.Values.Add(BotConstants.STEP_VALUES_NEW_INSULT_KEY, newInsultQnA);

            var askToHelp = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_NEW_INSULT_START, _brain.GetBastardDBContext());
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(string.Format(askToHelp, newInsultQnA.Insults.First())),
                RetryPrompt = MessageFactory.Text("Need some actual text here")
            }, cancellationToken);


        }

        private async Task<DialogTurnResult> PromptReplyToInsult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Options;

            // First time around. Ask for response
            bool addInsult = (bool)stepContext.Result;
            if (addInsult)
            {

                var reply = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_NEW_INSULT_REPLY, _brain.GetBastardDBContext());
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text(string.Format(reply, newInsultQnA.Insults.Last())),
                    RetryPrompt = MessageFactory.Text("Seriously, what should I say?")
                });
            }
            else
            {
                // User doesn't want to add it. Abort. Next prompt will have this text

                var reply = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_ENDING_DISAPPOINT, _brain.GetBastardDBContext());
                return await stepContext.EndDialogAsync(reply, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AddInsultResponse(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Values[BotConstants.STEP_VALUES_NEW_INSULT_KEY];

            string insultReply = (string)stepContext.Result;
            newInsultQnA.InsultResponse = insultReply;

            // Send adaptive card with new QnA
            await CommonDialogMessages.SendConfirmQnACard(stepContext, cancellationToken, newInsultQnA);

            var text = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_MORE_INSULTS_QUESTION, _brain.GetBastardDBContext());

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text(text),
                RetryPrompt = MessageFactory.Text($"Seriously, what other insults should I reply '{newInsultQnA.InsultResponse}' to?")
            });
        }

        private async Task<DialogTurnResult> MoreResponses(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Values[BotConstants.STEP_VALUES_NEW_INSULT_KEY];

            bool moreInsults = (bool)stepContext.Result;
            if (moreInsults)
            {
                // User wants to add more insults for response. Go around again, but pass the current insult state
                return await stepContext.BeginDialogAsync(nameof(AddMoreInsultsForResponseDialog), newInsultQnA, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> ConfirmNewInsultQnA(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Values[BotConstants.STEP_VALUES_NEW_INSULT_KEY];

            // Confirm it's saved
            var reply = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_INSULT_ADDED, _brain.GetBastardDBContext());
            await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(string.Format(reply, newInsultQnA.InsultResponse)),
                    cancellationToken
                    );

            // New new insult QnA to SQL
            await _brain.AddNewInsultQnA(newInsultQnA);

            // Done
            var text = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_ALL_DONE, _brain.GetBastardDBContext());
            await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(text),
                    cancellationToken
                    );

            // For next main diag loop, preface with this...
            return await stepContext.EndDialogAsync("Hit me again", cancellationToken);

        }

        private Task<bool> InsultResponseValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                return Task.FromResult(DataUtils.IsValidInsult(promptContext.Recognized.Value));
            }
            return Task.FromResult(false);
        }
    }
}
