using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BastardBot.Common.DB
{
    public class DbInitialiser
    {
        /// <summary>
        /// Seeds database if needed. Returns if seeding was needed.
        /// </summary>
        public async static Task<bool> Init(BastardDBContext context)
        {
            context.Database.EnsureCreated();

            context.Database.Migrate();

            // Look for any data.
            if (context.ChatPhrases.Any()) return false;   // DB has been seeded. Exit

            // Basic insult training
            var fuckYouToo = new InsultResponse("Fuck you too.");
            fuckYouToo.InsultTriggers.Add(new Insult("Fuck you"));
            fuckYouToo.InsultTriggers.Add(new Insult("Fuck you cunt"));
            fuckYouToo.InsultTriggers.Add(new Insult("Go fuck yourself"));

            var fuckYourMumResponse = new InsultResponse("Good choice. At least she's hot");
            fuckYourMumResponse.InsultTriggers.Add(new Insult("I fucked your mum"));
            fuckYourMumResponse.InsultTriggers.Add(new Insult("I have fucked your mother"));
            fuckYourMumResponse.InsultTriggers.Add(new Insult("I'm gonna fuck your mum"));
            fuckYourMumResponse.InsultTriggers.Add(new Insult("Fuck your mum"));
            fuckYourMumResponse.InsultTriggers.Add(new Insult("Fuck your mother"));

            var uSuckResponse = new InsultResponse("You suck harder, fag.");
            uSuckResponse.InsultTriggers.Add(new Insult("You suck"));
            uSuckResponse.InsultTriggers.Add(new Insult("You suck dick"));
            uSuckResponse.InsultTriggers.Add(new Insult("You fucking suck"));

            var cuntResponse = new InsultResponse("You're the cunt!");
            cuntResponse.InsultTriggers.Add(new Insult("Cunt"));
            cuntResponse.InsultTriggers.Add(new Insult("What a cunt"));


            context.NewResponses.AddRange(fuckYouToo, fuckYourMumResponse, uSuckResponse, cuntResponse);


            // Phrases & categories
            var newInsultReplyCategory = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_NEW_INSULT_REPLY };
            var newInsultCategoryStart = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_NEW_INSULT_START };
            var newInsultCategoryDisappointedEnding = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_ENDING_DISAPPOINT };
            var newInsultCategoryMoreInsults = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_MORE_INSULTS_QUESTION };
            var newInsultCategoryNextInsult = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_NEXT_INSULT_PROMPT };
            var newInsultCategoryInsultAdded = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_INSULT_ADDED };
            var newInsultCategoryAllDone = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_ALL_DONE };
            var newInsultCategoryAsIsed = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_AS_I_SAID_BEFORE };

            var newInsultCategoryLikedInsult = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_INSULT_LIKED };
            var newInsultCategoryDislikedInsult = new PhraseCategory { Text = BotConstants.CHAT_CATEGORY_INSULT_DISLIKED };

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Glad you liked it",
                Category = newInsultCategoryLikedInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Insults rock! Give me a good one.",
                Category = newInsultCategoryLikedInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = ":)",
                Category = newInsultCategoryLikedInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = ":D",
                Category = newInsultCategoryLikedInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "lol and maybe even rofel",
                Category = newInsultCategoryLikedInsult
            });

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Teach me a better one then, cunt.",
                Category = newInsultCategoryDislikedInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Fuck you. Teach me a better insult then. Bring it.",
                Category = newInsultCategoryDislikedInsult
            });

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "As I said before",
                Category = newInsultCategoryAsIsed
            }); 
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Once again",
                Category = newInsultCategoryAsIsed
            });

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "So what should I say if someone tells me '{0}'? Type it below...",
                Category = newInsultReplyCategory
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "What should I say if someone says '{0}'? Type it below...",
                Category = newInsultReplyCategory
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "So some cunt tells me '{0}' - what do I say back? Type it below...",
                Category = newInsultReplyCategory
            }); 
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "So some other twunt tells me '{0}' - what do I say back? Type it below...",
                Category = newInsultReplyCategory
            });

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "'{0}' - that's a new one. " +
                    "Want to add it to my database - you could help me be a better bastard if you've got a minute?",
                Category = newInsultCategoryStart
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Maybe I'm a n00b because '{0}' I've not heard before. " +
                    "Want to train me up on that one?",
                Category = newInsultCategoryStart
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "I've not heard '{0}' before. " +
                     "Want to add it to my database - you could help me be a better bastard if you've got a minute?",
                Category = newInsultCategoryStart
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "'{0}'? New one to my virtual eyes at least. " +
                     "Want to add it to my database - you could help me be a better bastard if you've got a minute?",
                Category = newInsultCategoryStart
            });

            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "I'm mildly disappointed in this outcome. Mildly.",
                Category = newInsultCategoryDisappointedEnding
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Well suck a dick then",
                Category = newInsultCategoryDisappointedEnding
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Oh",
                Category = newInsultCategoryDisappointedEnding
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Ok then",
                Category = newInsultCategoryDisappointedEnding
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Well ok then",
                Category = newInsultCategoryDisappointedEnding
            });


            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "More insults for that response?",
                Category = newInsultCategoryMoreInsults
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Any more insults for that response?",
                Category = newInsultCategoryMoreInsults
            });


            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "I'll add that one to the list. In 5-10 minutes once I've retrained my bastard brain, I'll be even more of a bastard thanks to you!",
                Category = newInsultCategoryInsultAdded
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "I've put that one to the list to teach myself. In 5-10 minutes once I've retrained my bastard brain, I'll be even more of a bastard thanks to you!",
                Category = newInsultCategoryInsultAdded
            });


            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "All done. Now, where were we...ah yes...",
                Category = newInsultCategoryAllDone
            }); 
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Nice one. Now, where were we...ah yes...",
                Category = newInsultCategoryAllDone
            });


            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "Next insult that I should say '{0}' back then?",
                Category = newInsultCategoryNextInsult
            });
            context.ChatPhrases.Add(new DialogPhrase
            {
                Text = "What's the next insult that I should say '{0}' back for then?",
                Category = newInsultCategoryNextInsult
            });


            await context.SaveChangesAsync();
            return true;
        }

    }
}
