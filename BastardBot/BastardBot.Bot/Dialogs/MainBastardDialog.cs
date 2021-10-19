// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BastardBot.Common;
using BastardBot.Common.DB;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BastardBot.Bot.Dialogs
{
    public class MainBastardDialog : ComponentDialog
    {
        private readonly BastardRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        IBotServices _botServices;
        protected BastardBrain _brain;
        private IMemoryCache _cache;

        private Random randomSelecta = new Random();

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainBastardDialog(BastardRecognizer luisRecognizer, ILogger<MainBastardDialog> logger,
            IBotServices botBervices, IServiceScopeFactory scopeFactory, DIBastardBrain brain,
            IMemoryCache memoryCache)
            : base(nameof(MainBastardDialog))
        {
            _luisRecognizer = luisRecognizer;
            _botServices = botBervices;
            _brain = brain;
            Logger = logger;
            _cache = memoryCache;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShowConditions,
                CheckAccepted,
                IntroStepAsync,
                ActStepAsync,
                ShowQnAInsultAndGetAnother,
                GoRoundAgain
            }));
            AddDialog(new NewInsultQnADialog(brain));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Accept Conditions Steps

        private async Task<DialogTurnResult> ShowConditions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get state from options and save. If null, save anyway so dictionary calls won't have to check if key exists.
            MainDiagState state = (MainDiagState)stepContext.Options;
            stepContext.Values.Add(BotConstants.STEP_VALUES_MAIN_STATE_KEY, state);

            if (state == null || !state.AcceptedConditions)
            {
                // Send adaptive card with disclaimer
                var adaptiveCard = CardGenerator.GetDisclaimerCard();
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = adaptiveCard,
                };
                var promptMessageSummary = MessageFactory.Attachment(adaptiveCardAttachment, "Hi! I'm BastardBot. Before we begin...");
                await stepContext.Context.SendActivityAsync(promptMessageSummary, cancellationToken);

                // Check S'all Good, Man
                string msg = $"All good?";
                var promptMessage = MessageFactory.Text(msg, msg, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = promptMessage,
                    Choices = new List<Choice>
                {
                    new Choice{ Value = BotConstants.CHOICE_SAUL_GOODMAN },
                    new Choice{ Value = "I'm not Sure" }
                }
                }, cancellationToken);
            }
            else
            {
                // Not first time around. Skip disclaimer steps
                return await stepContext.NextAsync(new Choice(BotConstants.CHOICE_SAUL_GOODMAN), cancellationToken);
            }

        }

        private async Task<DialogTurnResult> CheckAccepted(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            MainDiagState state = (MainDiagState)stepContext.Values[BotConstants.STEP_VALUES_MAIN_STATE_KEY];
            if (state == null || !state.AcceptedConditions)
            {
                var response = (FoundChoice)stepContext.Result;
                if (response.Value == BotConstants.CHOICE_SAUL_GOODMAN)
                {

                    // Let battle commence!
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Awesome. My job is to be a bastard & insult whoever wants to chat to me."), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Insult me and I'll reply. If it's a new insult, you can teach me it if you want. Round 1: here goes!"), cancellationToken);

                    var startingInsult = await GetRandomTrainedInsult();

                    // Goad them to continue
                    var intro2 = new List<string>()
                    {
                        "Your turn.",
                        "Bring it.",
                        "Bring it on.",
                        "Fight me, bitch.",
                        "Now fight me.",
                        "That's right."
                    };
                    int toSkip2 = randomSelecta.Next(0, intro2.Count);
                    var randomFollowup = intro2.Skip(toSkip2).Take(1).First();

                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"'{startingInsult}'. {randomFollowup}"),
                        RetryPrompt = MessageFactory.Text("Seriously, bring it")
                    });
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Probably for the best. Top of the day to you!"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }
            else
            {
                // Not first time around. Skip disclaimer steps
                return await stepContext.NextAsync(new Choice(BotConstants.CHOICE_SAUL_GOODMAN), cancellationToken);
            }
        }

        #endregion

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Did the user type something before this new dialogue? Could've been an instruction after last dialogue finished...
            var lastActivity = stepContext.Context.Activity;
            if (lastActivity?.Text != null)
            {
                string lastMsg = (string)lastActivity.Text;
                return await stepContext.NextAsync(lastMsg, cancellationToken);
            }
            else
            {
                // If nothing typed before, ask
                string msg = "Alright you cunt. What do you want?";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions() { Prompt = MessageFactory.Text(msg) }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Hard-coded responses
            bool abortDialogue = await HandleHardCodedResponses(stepContext, cancellationToken);
            if (abortDialogue)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<BastardTalk>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case BastardTalk.Intent.InsultMe:

                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("Oh, an insult! Here's one someone taught me..."),
                        cancellationToken);

                    await GenerateInsultFromQnA(stepContext, cancellationToken);
                    return await GetQnAResponseFromInput(stepContext, cancellationToken);
                case BastardTalk.Intent.ComplementInsult:
                    var likedResponse = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_INSULT_LIKED, _brain.GetBastardDBContext());
                    return await stepContext.NextAsync(likedResponse, cancellationToken);
                case BastardTalk.Intent.DismissInsult:
                    var dislikedResponse = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_INSULT_LIKED, _brain.GetBastardDBContext());
                    return await stepContext.NextAsync(dislikedResponse, cancellationToken);
                default:
                    // Handle with QnA
                    return await GetQnAResponseFromInput(stepContext, cancellationToken);
            }

            // Should never arrive here
            throw new Exception("wtf");
        }

        private async Task<DialogTurnResult> ShowQnAInsultAndGetAnother(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string qnaInsultResult = (string)stepContext.Result;
            string msg = qnaInsultResult;

            // Get state from options and save last insult
            var state = (MainDiagState)stepContext.Values[BotConstants.STEP_VALUES_MAIN_STATE_KEY];

            // 1st time around, this won't be set
            if (state == null)
            {
                state = new MainDiagState();
                stepContext.Values[BotConstants.STEP_VALUES_MAIN_STATE_KEY] = state;
            }

            if (!string.IsNullOrEmpty(state.LastInsult) && state.LastInsult == qnaInsultResult)
            {
                state.LastInsult = qnaInsultResult;
                var asISaid = await PhraseGenerator.GetPhrase(BotConstants.CHAT_CATEGORY_AS_I_SAID_BEFORE, _brain.GetBastardDBContext());
                msg = $"{asISaid}, {qnaInsultResult}. Try another one. {GetRandomTrainedInsult()}";
            }
            // Remember last insult
            state.LastInsult = qnaInsultResult;

            // Ask for another insult, giving response to last insult supplied
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg)
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> GoRoundAgain(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = (MainDiagState)stepContext.Values[BotConstants.STEP_VALUES_MAIN_STATE_KEY];
            state.AcceptedConditions = true;
            return await stepContext.ReplaceDialogAsync(nameof(MainBastardDialog), state, cancellationToken);
        }


        private async Task<DialogTurnResult> GetQnAResponseFromInput(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new QnAMakerOptions { Top = 1 };

            // The actual call to the QnA Maker service.
            var response = await _botServices.QnAMakerService.GetAnswersAsync(stepContext.Context, options);
            if (response != null && response.Length > 0)
            {
                return await stepContext.NextAsync(response[0].Answer, cancellationToken);
            }
            else
            {
                // New insult?
                string insult = stepContext.Context.Activity.Text;
                return await stepContext.BeginDialogAsync(nameof(NewInsultQnADialog), NewInsultQnA.FromNewInsult(insult), cancellationToken);
            }

        }

        #region Privates

        private async Task GenerateInsultFromQnA(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var insults = await _brain.GetTrainedInsultsOnly();
            var randomInsult = GetRandomString(insults.Select(i => i.Text));
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(randomInsult), cancellationToken);
        }

        private async Task<string> GetRandomTrainedInsult()
        {
            const string CACHE_INSULTS = "InsultsList";
            List<Insult> insults = null;
            if (!_cache.TryGetValue(CACHE_INSULTS, out insults))
            {
                insults = await _brain.GetTrainedInsultsOnly();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                // Save data in cache.
                _cache.Set(CACHE_INSULTS, insults, cacheEntryOptions);
            }
            return GetRandomString(insults.Select(i => i.Text));
        }

        private string GetRandomString(IEnumerable<string> list)
        {

            int toSkip = randomSelecta.Next(0, list.Count());
            return list.Skip(toSkip).Take(1).First();
        }

        /// <summary>
        /// Check for hard-coded commands.
        /// </summary>
        private async Task<bool> HandleHardCodedResponses(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch (stepContext.Context.Activity.Text)
            {
                case BotConstants.CMD_RETRAIN:

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Retraining..."), cancellationToken);
                    await _brain.TrainAndPublishNewModel();

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Done."), cancellationToken);
                    return true;

                case BotConstants.CMD_INSULTS_AND_RESPONSES:
                    List<InsultResponse> savedInsultsAndResponses = await _brain.GetTrainedInsultResponses();
                    string savedInsultsAndResponsesMsg = JsonConvert.SerializeObject(savedInsultsAndResponses, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(savedInsultsAndResponsesMsg));
                    return true;

                case BotConstants.CMD_INSULTS_ONLY:
                    var savedInsults = await _brain.GetTrainedInsultsOnly();
                    string savedInsultsMsg = JsonConvert.SerializeObject(savedInsults, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(savedInsultsMsg));
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Type '{BotConstants.CMD_INSULTS_AND_RESPONSES}' to see responses too."));
                    return true;

                default:
                    break;
            }

            return false;
        }

        #endregion
    }
}
