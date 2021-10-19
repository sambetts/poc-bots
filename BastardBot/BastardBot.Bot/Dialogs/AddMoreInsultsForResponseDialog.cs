using BastardBot.Common;
using BastardBot.Common.DB;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BastardBot.Bot.Dialogs
{
    public class AddMoreInsultsForResponseDialog : ComponentDialog
    {
        private BastardDBContext _context;
        public AddMoreInsultsForResponseDialog(BastardDBContext context) : base(nameof(AddMoreInsultsForResponseDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] 
            {
                AskForNextInsult,
                AskIfMore,
                RepeatIfNeeded
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt), InsultValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            _context = context;
        }

        private async Task<DialogTurnResult> AskForNextInsult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Options;


            // Save new QnA to session
            stepContext.Values.Add(BotConstants.STEP_VALUES_NEW_INSULT_KEY, newInsultQnA);


            var reply = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_NEXT_INSULT_PROMPT, _context);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text(string.Format(reply, newInsultQnA.InsultResponse)),
                RetryPrompt = MessageFactory.Text("Seriously, what should I say?")
            });
        }
        private async Task<DialogTurnResult> AskIfMore(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string newInsult = (string)stepContext.Result;

            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Values[BotConstants.STEP_VALUES_NEW_INSULT_KEY];
            newInsultQnA.Insults.Add(newInsult);

            // Send adaptive card with new QnA
            await CommonDialogMessages.SendConfirmQnACard(stepContext, cancellationToken, newInsultQnA);


            var reply = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_MORE_INSULTS_QUESTION, _context);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(reply),
                RetryPrompt = MessageFactory.Text("Soooo...more?")
            }, cancellationToken);
        }

        private Task<DialogTurnResult> RepeatIfNeeded(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool moar = (bool)stepContext.Result;

            NewInsultQnA newInsultQnA = (NewInsultQnA)stepContext.Values[BotConstants.STEP_VALUES_NEW_INSULT_KEY];

            if (moar)
            {
                return stepContext.ReplaceDialogAsync(nameof(AddMoreInsultsForResponseDialog), newInsultQnA, cancellationToken);
            }
            else
            {
                return stepContext.EndDialogAsync(newInsultQnA, cancellationToken);
            }
        }


        private Task<bool> InsultValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                return Task.FromResult(DataUtils.IsValidInsult(promptContext.Recognized.Value));
            }
            return Task.FromResult(false);
        }
    }
}
