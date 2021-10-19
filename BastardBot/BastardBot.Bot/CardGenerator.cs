using AdaptiveCards;
using BastardBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BastardBot.Bot
{
    public class CardGenerator
    {
        public static AdaptiveCard GetDisclaimerCard()
        {

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 1))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    { Style = AdaptiveContainerStyle.Emphasis, Bleed = true, Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock("Disclaimer: Offensive Content Ahead") { Size = AdaptiveTextSize.Medium, Weight = AdaptiveTextWeight.Bolder }
                        }
                    },
                    new AdaptiveTextBlock($"I'm likely to say offensive things based on what people on the internet tell me to say. " +
                        $"If you're easily offended, this if your fair warning.") { Wrap = true },
                    new AdaptiveTextBlock($"You need to agree you're OK with this. " +
                        $"I was built for fun and nothing else. That cool?"){ Wrap = true }
                }
            };

            return card;
        }

        public static AdaptiveCard GetNewQnACard(NewInsultQnA newInsult)
        {
            var insultFacts = new List<AdaptiveFact>();
            for (int i = 0; i < newInsult.Insults.Count; i++)
            {
                var insult = newInsult.Insults[i];
                bool isLastInsult = (i < newInsult.Insults.Count - 1);
                if (isLastInsult)
                {
                    insultFacts.Add(new AdaptiveFact { Title = insult, Value = "...or..." });
                }
                else
                {
                    insultFacts.Add(new AdaptiveFact { Title = insult});
                }
            }

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 1))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    { Style = AdaptiveContainerStyle.Emphasis, Bleed = true, Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock("New Insult Q&A") { Size = AdaptiveTextSize.Medium, Weight = AdaptiveTextWeight.Bolder }
                        }
                    },
                    new AdaptiveTextBlock($"So if someone says to me:") { Wrap = true },
                    new AdaptiveFactSet{ Facts = insultFacts },
                    new AdaptiveTextBlock($"...I'll tell them back:") { Wrap = true },
                    new AdaptiveTextBlock(newInsult.InsultResponse) 
                    { 
                        Wrap = true, Weight = AdaptiveTextWeight.Bolder, Color = AdaptiveTextColor.Accent, Size = AdaptiveTextSize.Large 
                    },
                }
            };

            return card;
        }
    }
}
